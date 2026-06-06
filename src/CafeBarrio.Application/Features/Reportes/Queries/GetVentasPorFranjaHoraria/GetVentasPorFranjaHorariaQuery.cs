using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Reportes.Queries.GetVentasPorFranjaHoraria;
public record GetVentasPorFranjaHorariaQuery(int SedeId, string Periodo) : IRequest<Result<IReadOnlyList<VentasPorHoraDto>>>;
