using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Turnos.Commands.CerrarTurno;
public record CerrarTurnoCommand(int TurnoId, decimal MontoEfectivoCierto, string? Observaciones) : IRequest<Result<bool>>;
