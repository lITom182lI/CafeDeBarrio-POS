namespace MUIS_CORE.Exceptions;

public sealed class UnauthorizedException : DomainException
{
    public UnauthorizedException(string action)
        : base("Error.Unauthorized", $"No autorizado para ejecutar: {action}.") { }
}
