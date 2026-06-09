using CafeBarrio.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CafeBarrio.Application.Features.Transacciones.EventHandlers;

public class TransaccionCreadaEventHandler : INotificationHandler<TransaccionCreadaEvent>
{
    private readonly ILogger<TransaccionCreadaEventHandler> _logger;

    public TransaccionCreadaEventHandler(ILogger<TransaccionCreadaEventHandler> logger)
        => _logger = logger;

    public Task Handle(TransaccionCreadaEvent notification, CancellationToken ct)
    {
        _logger.LogInformation(
            "Transacción {TransaccionId} creada — Sede: {SedeId}, Total: {Total:C2}, Fecha: {Fecha:u}",
            notification.TransaccionId, notification.SedeId,
            notification.Total, notification.Fecha);
        return Task.CompletedTask;
    }
}
