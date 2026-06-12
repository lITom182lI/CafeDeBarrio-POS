namespace CafeBarrio.Application.Common.Helpers;

public static class MoneyRounding
{
    public static decimal Round(decimal amount) =>
        Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
