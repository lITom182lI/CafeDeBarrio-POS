using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Reportes.Queries.GetStockBajo;
public record GetStockBajoQuery() : IRequest<Result<IReadOnlyList<StockBajoDto>>>;
