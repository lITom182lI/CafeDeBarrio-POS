using MediatR;

namespace CafeBarrio.Application.Events;

public record TransaccionCreadaEvent(
    int TransaccionId,
    int SedeId,
    decimal Total,
    DateTime Fecha) : INotification;
