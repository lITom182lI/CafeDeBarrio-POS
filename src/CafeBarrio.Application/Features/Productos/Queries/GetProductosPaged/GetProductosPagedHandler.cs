using CafeBarrio.Application.Common.Interfaces;
using MediatR;
using MUIS_CORE.Pagination;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Productos.Queries.GetProductosPaged;

public class GetProductosPagedHandler : IRequestHandler<GetProductosPagedQuery, Result<PagedResult<ProductoDto>>>
{
    private readonly IProductoRepository _productos;

    public GetProductosPagedHandler(IProductoRepository productos)
        => _productos = productos;

    public async Task<Result<PagedResult<ProductoDto>>> Handle(GetProductosPagedQuery request, CancellationToken ct)
    {
        var paged = await _productos.GetPagedAsync(
            new OffsetPaginationRequest(request.PageNumber, request.PageSize), ct);

        var dtos = paged.Items
            .Select(p => new ProductoDto(
                p.ProductoId, p.Nombre, p.Descripcion, p.Precio,
                p.CantidadDisponible, p.StockMinimo, p.UnidadMedida,
                p.CategoriaId, p.Categoria?.Nombre ?? string.Empty, p.EsMayorista, p.Activo))
            .ToList();

        return Result<PagedResult<ProductoDto>>.Success(
            new PagedResult<ProductoDto>(dtos, paged.TotalCount, paged.PageNumber, paged.PageSize));
    }
}
