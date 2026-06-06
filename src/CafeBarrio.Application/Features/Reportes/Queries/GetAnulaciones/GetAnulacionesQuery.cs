using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Reportes.Queries.GetAnulaciones;
public record GetAnulacionesQuery(int SedeId, string Periodo) : IRequest<Result<IReadOnlyList<AnulacionResumenDto>>>;
