using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Reportes.Queries.GetTopProductos;
public record GetTopProductosQuery(int SedeId, string Periodo, int Top = 5) : IRequest<Result<IReadOnlyList<TopProductoDto>>>;
