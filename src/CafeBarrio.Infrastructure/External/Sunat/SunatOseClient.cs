using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Common.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CafeBarrio.Infrastructure.External.Sunat;

public class SunatOseClient : ISunatService
{
    private readonly ISunatOseApiClient _ose;
    private readonly SunatOptions _options;
    private readonly ILogger<SunatOseClient> _log;

    public SunatOseClient(
        ISunatOseApiClient ose,
        IOptions<SunatOptions> options,
        ILogger<SunatOseClient> log)
    {
        _ose     = ose;
        _options = options.Value;
        _log     = log;
    }

    public async Task<EmitirBoletaResult> EmitirBoletaAsync(EmitirBoletaRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Ruc) || string.IsNullOrWhiteSpace(_options.OseToken))
        {
            _log.LogError("[SUNAT] Credenciales no configuradas (Sunat:Ruc o Sunat:OseToken vacíos).");
            return new EmitirBoletaResult(false, null,
                "Credenciales SUNAT no configuradas.", "Ruc o OseToken vacíos");
        }

        var tipoDoc      = MapTipoDocumento(req.TipoDocumento);
        var numDoc       = string.IsNullOrWhiteSpace(req.NumeroDocumento) ? "00000000" : req.NumeroDocumento;
        var denominacion = string.IsNullOrWhiteSpace(req.RazonSocial) ? "CLIENTE VARIOS" : req.RazonSocial;

        var items = req.Items.Select(i =>
        {
            var igvLinea   = MoneyRounding.Round(i.SubtotalLinea * _options.PorcentajeIgv / 100);
            var totalLinea = i.SubtotalLinea + igvLinea;
            return new OseBoletaItem(
                Codigo:         i.Nombre,
                Descripcion:    i.Nombre,
                Cantidad:       i.Cantidad,
                ValorUnitario:  i.PrecioUnitario,
                PrecioUnitario: MoneyRounding.Round(i.PrecioUnitario * (1 + _options.PorcentajeIgv / 100)),
                Subtotal:       i.SubtotalLinea,
                Igv:            igvLinea,
                Total:          totalLinea);
        }).ToList();

        var oseReq = new OseBoletaRequest(
            Serie:               _options.SerieBoletaBx,
            TipoDocCliente:      tipoDoc,
            NumDocCliente:       numDoc,
            DenominacionCliente: denominacion,
            Fecha:               req.Fecha,
            PorcentajeIgv:       _options.PorcentajeIgv,
            Items:               items,
            TotalGravadas:       req.Subtotal,
            TotalIgv:            req.Impuesto,
            Total:               req.Total);

        _log.LogInformation("[SUNAT] Emitiendo boleta transaccion {Id} — S/ {Total:F2}",
            req.TransaccionId, req.Total);

        var result = await _ose.EmitirBoletaAsync(oseReq, ct);

        if (!result.Aceptada)
        {
            _log.LogWarning("[SUNAT] Boleta rechazada transaccion {Id}: {Error}",
                req.TransaccionId, result.Error ?? result.Mensaje);
            return new EmitirBoletaResult(false, null,
                result.Mensaje ?? "Boleta no aceptada por el OSE",
                result.Error,
                result.Retryable);
        }

        _log.LogInformation("[SUNAT] Boleta aceptada: {NumeroSerie}", result.NumeroSerie);
        return new EmitirBoletaResult(true, result.NumeroSerie,
            result.Mensaje ?? "Boleta emitida exitosamente");
    }

    private static int MapTipoDocumento(string? tipo) => tipo?.ToUpperInvariant() switch
    {
        "DNI"       => 1,
        "CE"        => 4,
        "RUC"       => 6,
        "PASAPORTE" => 7,
        _           => 0
    };
}
