using CafeBarrio.Application.Features.Operadores.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Operadores.Commands.ValidarPin;

public record ValidarPinCommand(int OperadorId, string Pin) : IRequest<Result<OperadorLoginDto>>;
