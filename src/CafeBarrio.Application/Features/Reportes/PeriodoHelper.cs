namespace CafeBarrio.Application.Features.Reportes;

internal static class PeriodoHelper
{
    internal static (DateTime Desde, DateTime Hasta) Calcular(string periodo)
    {
        var hoy = DateTime.UtcNow.Date;
        return periodo.ToLowerInvariant() switch
        {
            "semana" => (hoy.AddDays(-6), hoy.AddDays(1).AddTicks(-1)),
            "mes"    => (new DateTime(hoy.Year, hoy.Month, 1), hoy.AddDays(1).AddTicks(-1)),
            _        => (hoy, hoy.AddDays(1).AddTicks(-1))
        };
    }
}
