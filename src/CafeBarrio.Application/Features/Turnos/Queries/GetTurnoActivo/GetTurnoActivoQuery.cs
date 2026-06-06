using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Turnos.Queries.GetTurnoActivo;
public record GetTurnoActivoQuery(int SedeId) : IRequest<Result<TurnoActivoDto?>>;
