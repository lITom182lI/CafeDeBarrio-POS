using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Anulaciones.Commands.CreateAnulacion;

public record CreateAnulacionCommand(
    int TransaccionId,
    string TipoAnulacion,
    string Motivo,
    decimal MontoDevuelto,
    string? MetodoDevolucion,
    int OperadorSolicitanteId,
    int AutorizadorId,
    bool ImpactoInventario = true
) : IRequest<Result<int>>;
