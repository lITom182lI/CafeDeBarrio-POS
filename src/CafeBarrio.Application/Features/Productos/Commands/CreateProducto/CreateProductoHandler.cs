using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Productos.Commands.CreateProducto;

public class CreateProductoHandler : IRequestHandler<CreateProductoCommand, Result<int>>
{
    private readonly IProductoRepository _productos;
    private readonly IUnitOfWork _uow;

    public CreateProductoHandler(IProductoRepository productos, IUnitOfWork uow)
    {
        _productos = productos;
        _uow = uow;
    }

    public async Task<Result<int>> Handle(CreateProductoCommand r, CancellationToken ct)
    {
        var producto = new Producto
        {
            Nombre               = r.Nombre,
            Descripcion          = r.Descripcion,
            Costo                = r.Costo,
            Precio               = r.Precio,
            CantidadDisponible   = r.CantidadDisponible,
            StockMinimo          = r.StockMinimo,
            UnidadMedida         = r.UnidadMedida,
            SeguimientoInventario = r.SeguimientoInventario,
            EsMayorista          = r.EsMayorista,
            CategoriaId          = r.CategoriaId,
            Activo               = true
        };
        await _productos.AddAsync(producto, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(producto.ProductoId);
    }
}
