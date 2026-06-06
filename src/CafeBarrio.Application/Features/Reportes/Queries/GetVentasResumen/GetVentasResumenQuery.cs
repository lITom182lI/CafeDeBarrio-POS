using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Reportes.Queries.GetVentasResumen;
public record GetVentasResumenQuery(int SedeId, string Periodo) : IRequest<Result<VentasResumenDto>>;
