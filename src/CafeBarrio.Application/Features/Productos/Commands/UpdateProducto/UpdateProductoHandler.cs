using CafeBarrio.Application.Common.Interfaces;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Productos.Commands.UpdateProducto;

public class UpdateProductoHandler : IRequestHandler<UpdateProductoCommand, Result>
{
    private readonly IProductoRepository _productos;
    private readonly IUnitOfWork _uow;

    public UpdateProductoHandler(IProductoRepository productos, IUnitOfWork uow)
    {
        _productos = productos;
        _uow = uow;
    }

    public async Task<Result> Handle(UpdateProductoCommand r, CancellationToken ct)
    {
        var producto = await _productos.GetByIdAsync(r.ProductoId, ct);
        if (producto is null)
            return Result.Failure(new Error("Producto.NotFound", $"Producto {r.ProductoId} no encontrado."));

        producto.Nombre               = r.Nombre;
        producto.Descripcion          = r.Descripcion;
        producto.Costo                = r.Costo;
        producto.Precio               = r.Precio;
        producto.CantidadDisponible   = r.CantidadDisponible;
        producto.StockMinimo          = r.StockMinimo;
        producto.UnidadMedida         = r.UnidadMedida;
        producto.SeguimientoInventario = r.SeguimientoInventario;
        producto.EsMayorista          = r.EsMayorista;
        producto.CategoriaId          = r.CategoriaId;
        producto.Activo               = r.Activo;

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
