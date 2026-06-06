using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Reportes.Queries.GetVentasPorMetodoPago;
public record GetVentasPorMetodoPagoQuery(int SedeId, string Periodo) : IRequest<Result<IReadOnlyList<VentasPorMetodoPagoDto>>>;
