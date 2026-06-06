using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Turnos.Commands.AbrirTurno;
public record AbrirTurnoCommand(int SedeId, int OperadorId, decimal MontoApertura) : IRequest<Result<int>>;
