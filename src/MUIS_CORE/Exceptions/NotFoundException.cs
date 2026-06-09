namespace MUIS_CORE.Exceptions;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key)
        : base("Error.NotFound", $"{entityName} con id '{key}' no fue encontrado.") { }
}
