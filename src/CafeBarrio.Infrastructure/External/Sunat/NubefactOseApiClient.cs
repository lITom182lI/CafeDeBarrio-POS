using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CafeBarrio.Infrastructure.External.Sunat;

public class NubefactOseApiClient : ISunatOseApiClient
{
    private readonly HttpClient _http;
    private readonly SunatOptions _options;
    private readonly ILogger<NubefactOseApiClient> _log;

    public NubefactOseApiClient(
        HttpClient http,
        IOptions<SunatOptions> options,
        ILogger<NubefactOseApiClient> log)
    {
        _http    = http;
        _options = options.Value;
        _log     = log;
    }

    public async Task<OseEmisionResult> EmitirBoletaAsync(OseBoletaRequest req, CancellationToken ct)
    {
        var payload = BuildPayload(req);
        var json    = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var url     = $"{_options.OseApiUrl}/{_options.Ruc}/boletas";

        _log.LogInformation("[NUBEFACT] POST {Url} — serie {Serie}", url, req.Serie);

        using var response = await _http.PostAsync(url, content, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            var statusCode = (int)response.StatusCode;
            _log.LogWarning("[NUBEFACT] HTTP {Status} — {Body}", statusCode, body);
            bool retryable = statusCode >= 500;
            return new OseEmisionResult(false, null, null,
                $"HTTP {statusCode}: {body}", retryable);
        }

        return ParseResponse(body);
    }

    private static NubefactBoletaPayload BuildPayload(OseBoletaRequest req) =>
        new()
        {
            TipoDeComprobante         = 2,
            Serie                     = req.Serie,
            ClienteTipoDeDocumento    = req.TipoDocCliente,
            ClienteNumeroDeDocumento  = req.NumDocCliente,
            ClienteDenominacion       = req.DenominacionCliente,
            FechaDeEmision            = req.Fecha.ToString("dd/MM/yyyy"),
            HoraDeEmision             = req.Fecha.ToString("HH:mm"),
            PorcentajeDeIgv           = (double)req.PorcentajeIgv,
            TotalGravadas             = req.TotalGravadas,
            TotalIgv                  = req.TotalIgv,
            Total                     = req.Total,
            Items = req.Items.Select(i => new NubefactItem
            {
                Codigo         = i.Codigo,
                Descripcion    = i.Descripcion,
                Cantidad       = i.Cantidad,
                ValorUnitario  = i.ValorUnitario,
                PrecioUnitario = i.PrecioUnitario,
                Subtotal       = i.Subtotal,
                Igv            = i.Igv,
                Total          = i.Total,
            }).ToList()
        };

    private static OseEmisionResult ParseResponse(string body)
    {
        try
        {
            using var doc  = JsonDocument.Parse(body);
            var root       = doc.RootElement;
            var aceptada   = root.TryGetProperty("aceptada_por_sunat", out var a) && a.GetBoolean();
            var serie      = root.TryGetProperty("serie",  out var s) ? s.GetString() : null;
            var numero     = root.TryGetProperty("numero", out var n) ? (int?)n.GetInt32() : null;
            var mensaje    = root.TryGetProperty("descripcion_respuesta", out var m) ? m.GetString() : null;
            var numSerie   = serie is not null && numero is not null ? $"{serie}-{numero}" : null;

            return new OseEmisionResult(aceptada, numSerie, mensaje, null);
        }
        catch (JsonException ex)
        {
            return new OseEmisionResult(false, null, null, $"Error parseando respuesta OSE: {ex.Message}");
        }
    }
}

// DTOs internos — solo visibles en este archivo
internal sealed class NubefactBoletaPayload
{
    [JsonPropertyName("operacion")]
    public string Operacion { get; init; } = "generar_comprobante";

    [JsonPropertyName("tipo_de_comprobante")]
    public int TipoDeComprobante { get; init; }

    [JsonPropertyName("serie")]
    public string Serie { get; init; } = "";

    [JsonPropertyName("numero")]
    public string Numero { get; init; } = "";

    [JsonPropertyName("sunat_transaction")]
    public int SunatTransaction { get; init; } = 1;

    [JsonPropertyName("cliente_tipo_de_documento")]
    public int ClienteTipoDeDocumento { get; init; }

    [JsonPropertyName("cliente_numero_de_documento")]
    public string ClienteNumeroDeDocumento { get; init; } = "";

    [JsonPropertyName("cliente_denominacion")]
    public string ClienteDenominacion { get; init; } = "";

    [JsonPropertyName("fecha_de_emision")]
    public string FechaDeEmision { get; init; } = "";

    [JsonPropertyName("hora_de_emision")]
    public string HoraDeEmision { get; init; } = "";

    [JsonPropertyName("moneda")]
    public int Moneda { get; init; } = 1;

    [JsonPropertyName("porcentaje_de_igv")]
    public double PorcentajeDeIgv { get; init; } = 18.0;

    [JsonPropertyName("total_gravadas")]
    public decimal TotalGravadas { get; init; }

    [JsonPropertyName("total_igv")]
    public decimal TotalIgv { get; init; }

    [JsonPropertyName("total")]
    public decimal Total { get; init; }

    [JsonPropertyName("enviar_automaticamente_a_la_sunat")]
    public bool EnviarAutomaticamenteALaSunat { get; init; } = true;

    [JsonPropertyName("enviar_automaticamente_al_cliente")]
    public bool EnviarAutomaticamenteAlCliente { get; init; } = false;

    [JsonPropertyName("items")]
    public List<NubefactItem> Items { get; init; } = [];
}

internal sealed class NubefactItem
{
    [JsonPropertyName("unidad_de_medida")]
    public string UnidadDeMedida { get; init; } = "NIU";

    [JsonPropertyName("codigo")]
    public string Codigo { get; init; } = "";

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; init; } = "";

    [JsonPropertyName("cantidad")]
    public int Cantidad { get; init; }

    [JsonPropertyName("valor_unitario")]
    public decimal ValorUnitario { get; init; }

    [JsonPropertyName("precio_unitario")]
    public decimal PrecioUnitario { get; init; }

    [JsonPropertyName("subtotal")]
    public decimal Subtotal { get; init; }

    [JsonPropertyName("tipo_de_igv")]
    public int TipoDeIgv { get; init; } = 1;

    [JsonPropertyName("igv")]
    public decimal Igv { get; init; }

    [JsonPropertyName("total")]
    public decimal Total { get; init; }

    [JsonPropertyName("anticipo_regularizacion")]
    public bool AnticipoRegularizacion { get; init; } = false;
}
