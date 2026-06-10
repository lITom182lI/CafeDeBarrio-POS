using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Transacciones.Commands.AnularTransaccion;

public class AnularTransaccionHandler : IRequestHandler<AnularTransaccionCommand, Result>
{
    private readonly ITransaccionRepository _transacciones;
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;
    private readonly IProductoRepository _productos;
    private readonly IUnitOfWork _uow;

    public AnularTransaccionHandler(
        ITransaccionRepository transacciones,
        IUsuarioRepository usuarios,
        IPasswordHasher hasher,
        IProductoRepository productos,
        IUnitOfWork uow)
    {
        _transacciones = transacciones;
        _usuarios = usuarios;
        _hasher = hasher;
        _productos = productos;
        _uow = uow;
    }

    public async Task<Result> Handle(AnularTransaccionCommand request, CancellationToken ct)
    {
        // Validar credenciales de Admin
        var admin = await _usuarios.GetByEmailAsync(request.AdminEmail, ct);
        if (admin is null || !_hasher.Verify(request.AdminPassword, admin.PasswordHash))
        {
            return Result.Failure(new Error("Auth.InvalidCredentials", "Credenciales de administrador inválidas."));
        }

        if (admin.Rol != "Admin")
        {
            return Result.Failure(new Error("Auth.Unauthorized", "El usuario no tiene permisos de administrador."));
        }

        // Obtener transacción
        var transaccion = await _transacciones.GetWithDetallesAndAnulacionAsync(request.TransaccionId, ct);
        if (transaccion is null)
        {
            return Result.Failure(new Error("Transaccion.NotFound", $"Transacción {request.TransaccionId} no encontrada."));
        }

        if (transaccion.Anulacion is not null)
        {
            return Result.Failure(new Error("Transaccion.AlreadyCancelled", "La transacción ya se encuentra anulada."));
        }

        // Registrar Anulación
        var anulacion = new Anulacion
        {
            TransaccionId = transaccion.TransaccionId,
            TipoAnulacion = "Total",
            Motivo = request.Motivo,
            MontoOriginal = transaccion.Total,
            MontoDevuelto = transaccion.Total,
            MetodoDevolucion = transaccion.MetodoPagoId.ToString(),
            OperadorSolicitanteId = request.OperadorSolicitanteId,
            AutorizadorId = admin.UsuarioId,
            FechaHora = DateTime.UtcNow,
            ImpactoInventario = true
        };

        transaccion.Anulacion = anulacion;

        // Restaurar inventario
        foreach (var detalle in transaccion.Detalles)
        {
            var producto = await _productos.GetByIdAsync(detalle.ProductoId, ct);
            if (producto is not null && producto.SeguimientoInventario)
            {
                producto.CantidadDisponible += detalle.Cantidad;
                await _productos.UpdateAsync(producto, ct);
            }
        }

        await _transacciones.UpdateAsync(transaccion, ct);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
