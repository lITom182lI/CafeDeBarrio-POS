namespace CafeBarrio.Application.Common.Exceptions;

public sealed class ConcurrencyException : Exception
{
    public ConcurrencyException()
        : base("Se detectó un conflicto de concurrencia. Reintenta la operación.") { }
}
