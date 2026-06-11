using CafeBarrio.Infrastructure.External.Sunat;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace CafeBarrio.Infrastructure.HealthChecks;

public class SunatHealthCheck : IHealthCheck
{
    private readonly SunatOptions _options;
    private readonly IHttpClientFactory _httpFactory;

    public SunatHealthCheck(IOptions<SunatOptions> options, IHttpClientFactory httpFactory)
    {
        _options     = options.Value;
        _httpFactory = httpFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken ct = default)
    {
        if (!_options.Enabled)
            return HealthCheckResult.Healthy("SUNAT en modo stub (integración deshabilitada).");

        if (string.IsNullOrWhiteSpace(_options.Ruc) || string.IsNullOrWhiteSpace(_options.OseToken))
            return HealthCheckResult.Degraded("Credenciales SUNAT no configuradas (Ruc u OseToken vacíos).");

        try
        {
            using var http    = _httpFactory.CreateClient();
            http.Timeout      = TimeSpan.FromSeconds(5);
            using var request = new HttpRequestMessage(HttpMethod.Head, _options.OseApiUrl);
            using var resp    = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            return resp.IsSuccessStatusCode || resp.StatusCode == System.Net.HttpStatusCode.Unauthorized
                ? HealthCheckResult.Healthy($"OSE alcanzable (HTTP {(int)resp.StatusCode}).")
                : HealthCheckResult.Degraded($"OSE respondió HTTP {(int)resp.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"OSE no alcanzable: {ex.Message}");
        }
    }
}
