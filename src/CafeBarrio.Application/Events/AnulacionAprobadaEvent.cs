using MediatR;

namespace CafeBarrio.Application.Events;

public record AnulacionAprobadaEvent(
    int AnulacionId,
    int TransaccionId,
    decimal MontoDevuelto,
    string TipoAnulacion) : INotification;
