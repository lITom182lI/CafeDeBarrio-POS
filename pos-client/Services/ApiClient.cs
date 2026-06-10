using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CafeBarrio.POS.Dtos;

namespace CafeBarrio.POS.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly string _base;
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ApiClient(string baseUrl, bool acceptSelfSigned = false)
    {
        _base = baseUrl.TrimEnd('/');
        var handler = new HttpClientHandler();
        if (acceptSelfSigned)
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        _http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) };
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var r = await _http.GetAsync($"{_base}/api/productos");
            return r.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<List<ProductoDto>> GetProductosAsync()
    {
        var allProducts = new List<ProductoDto>();
        int pageNumber = 1;
        int pageSize = 100;
        while (true)
        {
            var r = await _http.GetAsync($"{_base}/api/productos?pageNumber={pageNumber}&pageSize={pageSize}");
            r.EnsureSuccessStatusCode();
            var json = await r.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            List<ProductoDto> items;
            if (doc.RootElement.TryGetProperty("items", out var itemsElement))
                items = JsonSerializer.Deserialize<List<ProductoDto>>(itemsElement.GetRawText(), _json) ?? [];
            else
                items = JsonSerializer.Deserialize<List<ProductoDto>>(json, _json) ?? [];

            allProducts.AddRange(items);
            if (items.Count < pageSize) break;
            pageNumber++;
        }
        return allProducts;
    }

    public async Task<List<MetodoPagoDto>> GetMetodosPagoAsync()
    {
        var r = await _http.GetAsync($"{_base}/api/metodos-pago");
        r.EnsureSuccessStatusCode();
        var json = await r.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<MetodoPagoDto>>(json, _json)
               ?? [];
    }

    public async Task<int> PostTransaccionAsync(CreateTransaccionRequest request)
    {
        var body = JsonSerializer.Serialize(request, _json);
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        var r = await _http.PostAsync($"{_base}/api/transacciones", content);
        r.EnsureSuccessStatusCode();
        var json = await r.Content.ReadAsStringAsync();
        // El API devuelve el TransaccionId directamente (Result<int>)
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("value", out var v)
            ? v.GetInt32()
            : doc.RootElement.GetInt32();
    }

    public async Task<List<OperadorDto>> GetOperadoresAsync()
    {
        try
        {
            var r = await _http.GetAsync($"{_base}/api/operadores");
            if (!r.IsSuccessStatusCode) return [];
            var json = await r.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<OperadorDto>>(json, _json) ?? [];
        }
        catch { return []; }
    }

    public async Task<OperadorLoginResult?> ValidarPinAsync(int operadorId, string pin)
    {
        var body    = JsonSerializer.Serialize(new { OperadorId = operadorId, Pin = pin }, _json);
        var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        var r       = await _http.PostAsync($"{_base}/api/operadores/validar-pin", content);
        if (!r.IsSuccessStatusCode) return null;
        var json = await r.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OperadorLoginResult>(json, _json);
    }
}

public record OperadorLoginResult(int OperadorId, string Nombre);
public record OperadorDto(int OperadorId, string Nombre);
