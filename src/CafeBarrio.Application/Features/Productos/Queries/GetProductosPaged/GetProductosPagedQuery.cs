using MediatR;
using MUIS_CORE.Pagination;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Productos.Queries.GetProductosPaged;

public record GetProductosPagedQuery(int PageNumber, int PageSize) : IRequest<Result<PagedResult<ProductoDto>>>;
