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
    private readonly ITurnoRepository _turnos;

    public CreateTransaccionHandler(
        ITransaccionRepository transacciones,
        IProductoRepository productos,
        IConfiguracionNegocioRepository configuracion,
        IUnitOfWork uow,
        IPublisher publisher,
        IIdempotencyRecordRepository idempotencyRecords,
        ICurrentUserService currentUser,
        ITurnoRepository turnos)
    {
        _transacciones      = transacciones;
        _productos          = productos;
        _configuracion      = configuracion;
        _uow                = uow;
        _publisher          = publisher;
        _idempotencyRecords = idempotencyRecords;
        _currentUser        = currentUser;
        _turnos             = turnos;
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
        decimal totalBruto = 0;
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
            totalBruto += producto.Precio * item.Cantidad;
        }

        // Validar turno activo (si se especifica TurnoId)
        if (request.TurnoId is not null)
        {
            var turnoActivo = await _turnos.GetActivoBySedeAsync(request.SedeId, ct);
            if (turnoActivo is null)
                return Result<int>.Failure(new Error("Turno.SinTurnoAbierto",
                    "No hay un turno abierto para esta sede."));
            if (turnoActivo.TurnoId != request.TurnoId.Value)
                return Result<int>.Failure(new Error("Turno.TurnoInactivo",
                    "El turno especificado no es el turno activo para esta sede."));
        }

        // Validar monto de pago dividido (el primario no puede cubrir el total completo)
        if (request.MetodoPagoSecundarioId is not null && request.MontoMetodoPrimario is not null)
        {
            var totalEstimado = totalBruto; // ya es IGV-inclusivo
            if (request.MontoMetodoPrimario.Value >= totalEstimado)
                return Result<int>.Failure(new Error("Pago.MontoMetodoPrimarioExcedido",
                    "El monto del método primario cubre o excede el total. Use un solo método de pago."));
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
            }

            var divisor       = 1 + tasaIgv;
            var baseImponible = MoneyRounding.Round(totalBruto / divisor);
            var impuesto      = MoneyRounding.Round(totalBruto - baseImponible);

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
                Subtotal       = baseImponible,
                Impuesto       = impuesto,
                RecargoPropina = 0m,
                CostoEnvio     = 0m,
                Total          = totalBruto,
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
            // Si dos requests paralelos llegan con la misma key, el segundo golpea UX_IdempotencyRecords_Key.
            // En ese caso, UnitOfWork lanza PersistenceException; devolvemos el TransaccionId ya persistido.
            try
            {
                await _uow.SaveChangesAsync(ct);
                await _uow.CommitAsync(ct);
            }
            catch (CafeBarrio.Application.Common.Exceptions.PersistenceException)
            {
                await _uow.RollbackAsync(ct);
                var race = await _idempotencyRecords.GetByKeyAsync(request.IdempotencyKey, ct);
                if (race is not null)
                    return Result<int>.Success(race.TransaccionId);
                throw;
            }

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
