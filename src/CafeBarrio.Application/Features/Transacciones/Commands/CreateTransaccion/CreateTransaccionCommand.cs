using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Transacciones.Commands.CreateTransaccion;

public record CreateTransaccionItemDto(int ProductoId, int Cantidad);

public record CreateTransaccionCommand(
    int ClienteId,
    int SedeId,
    int MetodoPagoId,
    int? OpcionEnvioId,
    string Canal,
    IReadOnlyList<CreateTransaccionItemDto> Items,
    int? TurnoId = null,
    int? OperadorId = null
) : IRequest<Result<int>>;
