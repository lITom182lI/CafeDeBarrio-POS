using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Operadores.Commands.UpdateOperador;

public record UpdateOperadorCommand(
    int OperadorId,
    string Nombre,
    bool Activo,
    string? NuevoPin
) : IRequest<Result>;
