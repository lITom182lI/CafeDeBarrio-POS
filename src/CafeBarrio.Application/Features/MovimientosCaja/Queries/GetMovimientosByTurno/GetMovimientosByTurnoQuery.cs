using MediatR;
using MUIS_CORE.Wrappers;
using CafeBarrio.Application.Features.MovimientosCaja.Dtos;

namespace CafeBarrio.Application.Features.MovimientosCaja.Queries.GetMovimientosByTurno;

public record GetMovimientosByTurnoQuery(int TurnoId) : IRequest<Result<IReadOnlyList<MovimientoCajaDto>>>;
