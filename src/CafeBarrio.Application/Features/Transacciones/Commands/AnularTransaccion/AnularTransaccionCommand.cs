using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Transacciones.Commands.AnularTransaccion;

public record AnularTransaccionCommand(
    int TransaccionId,
    string Motivo,
    int OperadorSolicitanteId,
    string? MetodoDevolucion    // nombre del método: "Efectivo", "Yape", "Tarjeta", "Plin"
) : IRequest<Result>;
