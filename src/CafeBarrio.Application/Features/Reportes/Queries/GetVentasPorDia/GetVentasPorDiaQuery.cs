using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Reportes.Queries.GetVentasPorDia;

public record GetVentasPorDiaQuery(int SedeId, string Periodo) : IRequest<Result<IReadOnlyList<VentasPorDiaDto>>>;
