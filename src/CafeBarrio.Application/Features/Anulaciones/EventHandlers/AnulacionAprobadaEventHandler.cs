using CafeBarrio.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CafeBarrio.Application.Features.Anulaciones.EventHandlers;

public class AnulacionAprobadaEventHandler : INotificationHandler<AnulacionAprobadaEvent>
{
    private readonly ILogger<AnulacionAprobadaEventHandler> _logger;

    public AnulacionAprobadaEventHandler(ILogger<AnulacionAprobadaEventHandler> logger)
        => _logger = logger;

    public Task Handle(AnulacionAprobadaEvent notification, CancellationToken ct)
    {
        _logger.LogWarning(
            "Anulación {AnulacionId} aprobada — Transacción: {TransaccionId}, Tipo: {TipoAnulacion}, Devuelto: {MontoDevuelto:C2}",
            notification.AnulacionId, notification.TransaccionId,
            notification.TipoAnulacion, notification.MontoDevuelto);
        return Task.CompletedTask;
    }
}
