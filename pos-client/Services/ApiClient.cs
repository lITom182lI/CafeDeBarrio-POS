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

    public ApiClient(string baseUrl)
    {
        _base = baseUrl.TrimEnd('/');
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
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
        var r = await _http.GetAsync($"{_base}/api/productos?pageSize=1000");
        r.EnsureSuccessStatusCode();
        var json = await r.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("items", out var itemsElement))
            return JsonSerializer.Deserialize<List<ProductoDto>>(itemsElement.GetRawText(), _json) ?? [];
        return JsonSerializer.Deserialize<List<ProductoDto>>(json, _json) ?? [];
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
}
