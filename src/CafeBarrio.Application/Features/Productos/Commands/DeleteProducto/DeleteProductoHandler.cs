using CafeBarrio.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Productos.Commands.DeleteProducto;

public class DeleteProductoHandler : IRequestHandler<DeleteProductoCommand, Result>
{
    private readonly DbContext _context;

    public DeleteProductoHandler(DbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteProductoCommand request, CancellationToken cancellationToken)
    {
        var producto = await _context.Set<Producto>().FirstOrDefaultAsync(p => p.ProductoId == request.ProductoId, cancellationToken);
        if (producto == null)
        {
            return Result.Failure(new Error("Producto.NoEncontrado", "El producto no existe."));
        }

        try
        {
            _context.Set<Producto>().Remove(producto);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception)
        {
            // If it fails (e.g. FK constraint), we do a soft delete fallback
            producto.Activo = false;
            _context.Set<Producto>().Update(producto);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
