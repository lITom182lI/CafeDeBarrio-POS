using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Reportes.Queries.GetTurnosCerrados;

public record GetTurnosCerradosQuery(int SedeId, string Periodo = "mes") : IRequest<Result<IReadOnlyList<TurnoCerradoDto>>>;
