using CafeBarrio.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CafeBarrio.Infrastructure.External;

// Stub activo mientras el negocio no tenga RUC + certificado OSE.
// Para activar emisión real:
//   1. Implementar SunatOseClient (UBL 2.1 + firma XML + endpoint OSE)
//   2. Reemplazar este registro en DependencyInjection.cs
//   3. Cambiar Sunat:Enabled = true en appsettings.json
public class SunatStubService : ISunatService
{
    private readonly bool _enabled;
    private readonly string _serie;
    private readonly ILogger<SunatStubService> _log;
    private static int _correlativo = 1;

    public SunatStubService(IConfiguration cfg, ILogger<SunatStubService> log)
    {
        _enabled = bool.TryParse(cfg["Sunat:Enabled"], out var e) && e;
        _serie   = cfg["Sunat:SerieBoletaBx"] ?? "B001";
        _log     = log;
    }

    public Task<EmitirBoletaResult> EmitirBoletaAsync(EmitirBoletaRequest req, CancellationToken ct = default)
    {
        if (!_enabled)
        {
            _log.LogInformation(
                "[SUNAT-STUB] Transaccion {Id} — SUNAT no configurado. Boleta omitida.",
                req.TransaccionId);
            return Task.FromResult(new EmitirBoletaResult(
                Emitida: false,
                NumeroSerie: null,
                Mensaje: "SUNAT no habilitado. Configure RUC y OSE para emisión real."));
        }

        // Stub: simula emisión exitosa (útil para UAT antes de OSE real)
        var numero = $"{_serie}-{_correlativo++:D8}";
        _log.LogInformation(
            "[SUNAT-STUB] Transaccion {Id} — Boleta simulada: {Numero} S/ {Total:F2}",
            req.TransaccionId, numero, req.Total);
        return Task.FromResult(new EmitirBoletaResult(
            Emitida: true,
            NumeroSerie: numero,
            Mensaje: "Boleta simulada (stub). No enviada a SUNAT."));
    }
}
