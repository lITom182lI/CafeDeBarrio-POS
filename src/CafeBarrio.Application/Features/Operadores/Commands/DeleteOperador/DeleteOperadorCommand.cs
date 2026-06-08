using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Operadores.Commands.DeleteOperador;

public record DeleteOperadorCommand(int OperadorId) : IRequest<Result<bool>>;
