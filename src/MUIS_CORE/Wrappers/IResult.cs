namespace MUIS_CORE.Wrappers;

// Interfaz marcadora que permite al ValidationBehavior trabajar sin reflexión.
// Result y Result<T> implementan esta interfaz — hace posible la restricción
// where TResponse : IResult en el pipeline de MediatR.
public interface IResult
{
    bool IsSuccess { get; }
    bool IsFailure { get; }

    // Método estático de interfaz (C# 11+) — permite llamar Result<T>.Failure
    // sin reflexión, con seguridad en tiempo de compilación.
    static abstract IResult Failure(Error error);
}
