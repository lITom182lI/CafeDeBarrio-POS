namespace CafeBarrio.Application.Common.Exceptions;

public sealed class PersistenceException : Exception
{
    public PersistenceException()
        : base("No se pudo persistir el cambio. Puede existir una restricción de integridad referencial.") { }
}
