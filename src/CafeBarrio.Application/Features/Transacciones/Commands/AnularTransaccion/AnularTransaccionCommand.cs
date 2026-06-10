using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Transacciones.Commands.AnularTransaccion;

public record AnularTransaccionCommand(
    int TransaccionId,
    string Motivo,
    int OperadorSolicitanteId,
    string AdminEmail,
    string AdminPassword
) : IRequest<Result>;
