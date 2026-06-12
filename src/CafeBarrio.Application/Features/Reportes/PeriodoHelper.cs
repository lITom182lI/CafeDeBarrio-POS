using CafeBarrio.Domain.Common;

namespace CafeBarrio.Application.Features.Reportes;

internal static class PeriodoHelper
{
    private const int MaxDias = 92;

    internal static (DateTime Desde, DateTime Hasta) Calcular(string periodo)
    {
        var hoy = DateTime.UtcNow.Date;
        var (desde, hasta) = periodo.ToLowerInvariant() switch
        {
            PeriodoReporte.Semana => (hoy.AddDays(-6), hoy.AddDays(1).AddTicks(-1)),
            PeriodoReporte.Mes    => (new DateTime(hoy.Year, hoy.Month, 1), hoy.AddDays(1).AddTicks(-1)),
            _                     => (hoy, hoy.AddDays(1).AddTicks(-1))
        };

        if ((hasta - desde).TotalDays > MaxDias)
            desde = hasta.AddDays(-MaxDias);

        return (desde, hasta);
    }
}
