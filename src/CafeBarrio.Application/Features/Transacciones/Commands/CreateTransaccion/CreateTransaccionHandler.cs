using CafeBarrio.Application.Events;
using CafeBarrio.Application.Common.Helpers;
using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Transacciones.Commands.CreateTransaccion;

public class CreateTransaccionHandler : IRequestHandler<CreateTransaccionCommand, Result<int>>
{
    private readonly ITransaccionRepository _transacciones;
    private readonly IProductoRepository _productos;
    private readonly IConfiguracionNegocioRepository _configuracion;
    private readonly IUnitOfWork _uow;
    private readonly IPublisher _publisher;
    private readonly IIdempotencyRecordRepository _idempotencyRecords;
    private readonly ICurrentUserService _currentUser;

    public CreateTransaccionHandler(
        ITransaccionRepository transacciones,
        IProductoRepository productos,
        IConfiguracionNegocioRepository configuracion,
        IUnitOfWork uow,
        IPublisher publisher,
        IIdempotencyRecordRepository idempotencyRecords,
        ICurrentUserService currentUser)
    {
        _transacciones = transacciones;
        _productos     = productos;
        _configuracion = configuracion;
        _uow           = uow;
        _publisher     = publisher;
        _idempotencyRecords = idempotencyRecords;
        _currentUser   = currentUser;
    }

    public async Task<Result<int>> Handle(CreateTransaccionCommand request, CancellationToken ct)
    {
        // Verificar idempotencia
        var existing = await _idempotencyRecords.GetByKeyAsync(request.IdempotencyKey, ct);

        if (existing is not null)
            return Result<int>.Success(existing.TransaccionId);

        var config  = await _configuracion.GetActivaBySedeAsync(request.SedeId, ct);
        if (config is null)
            return Result<int>.Failure(new Error("Configuracion.NotFound", "No se encontró configuración activa para la sede."));

        if (_currentUser.SedeId is not null && _currentUser.SedeId != request.SedeId)
            return Result<int>.Failure(new Error("Auth.ForbiddenSede",
                "No tienes acceso a esta sede."));

        var tasaIgv = config.TasaIGV + config.TasaIPM;
        var detalles = new List<DetalleTransaccion>();
        decimal subtotal = 0;
        var productosDict = new Dictionary<int, Producto>();

        // 1. Validar stock para todos los ítems antes de hacer cambios
        foreach (var item in request.Items)
        {
            var producto = await _productos.GetByIdAsync(item.ProductoId, ct);
            if (producto is null)
                return Result<int>.Failure(new Error("Producto.NotFound",
                    $"Producto {item.ProductoId} no encontrado."));

            if (producto.SeguimientoInventario && producto.CantidadDisponible < item.Cantidad)
                return Result<int>.Failure(new Error("Stock.Insuficiente",
                    $"Stock insuficiente para {producto.Nombre}."));

            productosDict[item.ProductoId] = producto;
        }

        // 2. Transacción de Base de Datos
        await _uow.BeginTransactionAsync(ct);
        try
        {
            // Descontar en memoria
            foreach (var item in request.Items)
            {
                var producto = productosDict[item.ProductoId];
                var descuento = producto.DescontarStock(item.Cantidad);
                if (descuento.IsFailure)
                    return Result<int>.Failure(descuento.Errors[0]);

                var linea = new DetalleTransaccion
                {
                    ProductoId     = item.ProductoId,
                    Cantidad       = item.Cantidad,
                    PrecioUnitario = producto.Precio,
                    SubtotalLinea  = producto.Precio * item.Cantidad
                };
                detalles.Add(linea);
                subtotal += linea.SubtotalLinea;
            }

            var impuesto = MoneyRounding.Round(subtotal * tasaIgv);

            var transaccion = new Transaccion
            {
                ClienteId      = request.ClienteId,
                SedeId         = request.SedeId,
                MetodoPagoId   = request.MetodoPagoId,
                MetodoPagoSecundarioId = request.MetodoPagoSecundarioId,
                MontoMetodoPrimario    = request.MontoMetodoPrimario,
                OpcionEnvioId  = request.OpcionEnvioId,
                TurnoId        = request.TurnoId,
                OperadorId     = request.OperadorId,
                Canal          = request.Canal,
                EsMayorista    = false,
                Fecha          = DateTime.UtcNow,
                Subtotal       = subtotal,
                Impuesto       = impuesto,
                RecargoPropina = 0m,
                CostoEnvio     = 0m,
                Total          = subtotal + impuesto,
                Detalles       = detalles,
                TipoDocumento  = request.TipoDocumento,
                NumeroDocumento = request.NumeroDocumento,
                RazonSocial    = request.RazonSocial
            };

            await _transacciones.AddAsync(transaccion, ct);

            // Primera save: genera IDENTITY en transaccion.TransaccionId
            try
            {
                await _uow.SaveChangesAsync(ct);
            }
            catch (CafeBarrio.Application.Common.Exceptions.ConcurrencyException)
            {
                await _uow.RollbackAsync(ct);
                return Result<int>.Failure(new Error("Stock.ConcurrencyConflict",
                    "Conflicto de inventario. Reintenta la venta."));
            }

            await _idempotencyRecords.AddAsync(new IdempotencyRecord
            {
                IdempotencyKey = request.IdempotencyKey,
                TransaccionId  = transaccion.TransaccionId,   // ahora es el valor real
                CreatedAtUtc   = DateTime.UtcNow
            }, ct);

            // Segunda save: persiste el registro de idempotencia
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);

            await _publisher.Publish(new TransaccionCreadaEvent(
                transaccion.TransaccionId, transaccion.SedeId,
                transaccion.Total, transaccion.Fecha), ct);

            return Result<int>.Success(transaccion.TransaccionId);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
