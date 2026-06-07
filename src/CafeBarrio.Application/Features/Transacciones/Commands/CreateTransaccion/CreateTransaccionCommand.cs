using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Transacciones.Commands.CreateTransaccion;

public record CreateTransaccionItemDto(int ProductoId, int Cantidad);

public record CreateTransaccionCommand(
    int SedeId,
    int MetodoPagoId,
    IReadOnlyList<CreateTransaccionItemDto> Items,
    int? ClienteId        = null,
    int? OpcionEnvioId    = null,
    string Canal          = "POS",
    int? TurnoId          = null,
    int? OperadorId       = null,
    string? TipoDocumento  = null,
    string? NumeroDocumento = null,
    string? RazonSocial    = null,
    int? MetodoPagoSecundarioId = null,
    decimal? MontoMetodoPrimario = null
) : IRequest<Result<int>>;
