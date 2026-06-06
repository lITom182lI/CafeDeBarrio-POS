using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Operadores.Commands.CreateOperador;

public record CreateOperadorCommand(string Nombre, string Pin) : IRequest<Result<int>>;
