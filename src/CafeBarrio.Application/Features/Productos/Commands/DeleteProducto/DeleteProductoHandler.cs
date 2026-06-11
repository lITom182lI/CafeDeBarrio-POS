using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using MediatR;
using MUIS_CORE.Wrappers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CafeBarrio.Application.Features.Productos.Commands.DeleteProducto;

public class DeleteProductoHandler : IRequestHandler<DeleteProductoCommand, Result>
{
    private readonly IProductoRepository _productos;
    private readonly IUnitOfWork _uow;

    public DeleteProductoHandler(IProductoRepository productos, IUnitOfWork uow)
    {
        _productos = productos;
        _uow = uow;
    }

    public async Task<Result> Handle(DeleteProductoCommand request, CancellationToken cancellationToken)
    {
        var producto = await _productos.GetByIdAsync(request.ProductoId, cancellationToken);
        if (producto == null)
        {
            return Result.Failure(new Error("Producto.NoEncontrado", "El producto no existe."));
        }

        try
        {
            await _productos.DeleteAsync(producto.ProductoId, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception)
        {
            // If it fails (e.g. FK constraint), we do a soft delete fallback
            producto.Activo = false;
            await _productos.UpdateAsync(producto, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
