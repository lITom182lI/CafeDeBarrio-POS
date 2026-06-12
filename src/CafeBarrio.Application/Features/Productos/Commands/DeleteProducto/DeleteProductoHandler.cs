using CafeBarrio.Application.Common.Exceptions;
using CafeBarrio.Application.Common.Interfaces;
using MediatR;
using MUIS_CORE.Wrappers;
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
        if (producto is null)
            return Result.Failure(new Error("Producto.NoEncontrado", "El producto no existe."));

        try
        {
            await _productos.DeleteAsync(producto.ProductoId, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (CafeBarrio.Application.Common.Exceptions.PersistenceException)
        {
            // FK violation: producto tiene transacciones asociadas → soft delete
            producto.Activo = false;
            await _productos.UpdateAsync(producto, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        // Cualquier otra excepción: relanzar para que el middleware la capture y logee
    }
}
