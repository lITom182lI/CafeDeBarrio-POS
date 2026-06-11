namespace CafeBarrio.Domain.Common;

public static class PeriodoReporte
{
    public const string Dia        = "dia";
    public const string Semana     = "semana";
    public const string Mes        = "mes";
    public const string Turno      = "turno";

    private static readonly HashSet<string> _validos =
        new(StringComparer.OrdinalIgnoreCase)
        { Dia, Semana, Mes, Turno };

    public static bool EsValido(string? periodo) =>
        periodo is not null && _validos.Contains(periodo);
}
