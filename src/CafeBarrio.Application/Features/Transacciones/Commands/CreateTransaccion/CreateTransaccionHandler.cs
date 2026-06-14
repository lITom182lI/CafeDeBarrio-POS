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

    // Transporta un Result<int> de negocio fuera del lambda de ExecuteInTransactionAsync
    private sealed class BusinessFailureException(Result<int> result) : Exception
    {
        public Result<int> Result { get; } = result;
    }

    public async Task<Result<int>> Handle(CreateTransaccionCommand request, CancellationToken ct)
    {
        // Verificar idempotencia (fuera de transacción)
        var existing = await _idempotencyRecords.GetByKeyAsync(request.IdempotencyKey, ct);
        if (existing is not null)
            return Result<int>.Success(existing.TransaccionId);

        var config = await _configuracion.GetActivaBySedeAsync(request.SedeId, ct);
        if (config is null)
            return Result<int>.Failure(new Error("Configuracion.NotFound", "No se encontró configuración activa para la sede."));

        if (_currentUser.SedeId is not null && _currentUser.SedeId != request.SedeId)
            return Result<int>.Failure(new Error("Auth.ForbiddenSede", "No tienes acceso a esta sede."));

        var tasaIgv = config.TasaIGV + config.TasaIPM;

        // Todo el trabajo transaccional envuelto en ExecuteInTransactionAsync para compatibilidad
        // con SqlServerRetryingExecutionStrategy (no permite BeginTransaction manual sin esta envoltura).
        int transaccionId;
        decimal totalPublicado = 0m;
        try
        {
            transaccionId = await _uow.ExecuteInTransactionAsync(async (token) =>
            {
                var divisor = 1m + tasaIgv;
                var detalles = new List<DetalleTransaccion>();
                decimal subtotal = 0;

                // Re-fetch dentro de la transacción para seguridad en reintentos
                foreach (var item in request.Items)
                {
                    var producto = await _productos.GetByIdAsync(item.ProductoId, token);
                    if (producto is null)
                        throw new BusinessFailureException(
                            Result<int>.Failure(new Error("Producto.NotFound", $"Producto {item.ProductoId} no encontrado.")));

                    if (producto.SeguimientoInventario && producto.CantidadDisponible < item.Cantidad)
                        throw new BusinessFailureException(
                            Result<int>.Failure(new Error("Stock.Insuficiente", $"Stock insuficiente para {producto.Nombre}.")));

                    var descuento = producto.DescontarStock(item.Cantidad);
                    if (descuento.IsFailure)
                        throw new BusinessFailureException(Result<int>.Failure(descuento.Errors[0]));

                    detalles.Add(new DetalleTransaccion
                    {
                        ProductoId     = item.ProductoId,
                        Cantidad       = item.Cantidad,
                        PrecioUnitario = producto.Precio,
                        SubtotalLinea  = MoneyRounding.Round((producto.Precio / divisor) * item.Cantidad)
                    });
                    subtotal += producto.Precio * item.Cantidad;
                }

                if (request.TurnoId is not null)
                {
                    var turnoActivo = await _turnos.GetActivoBySedeAsync(request.SedeId, token);
                    if (turnoActivo is null)
                        throw new BusinessFailureException(
                            Result<int>.Failure(new Error("Turno.SinTurnoAbierto", "No hay un turno abierto para esta sede.")));
                    if (turnoActivo.TurnoId != request.TurnoId.Value)
                        throw new BusinessFailureException(
                            Result<int>.Failure(new Error("Turno.TurnoInactivo", "El turno especificado no es el turno activo para esta sede.")));
                }

                if (request.MetodoPagoSecundarioId is not null && request.MontoMetodoPrimario is not null)
                {
                    if (request.MontoMetodoPrimario.Value >= subtotal)
                        throw new BusinessFailureException(
                            Result<int>.Failure(new Error("Pago.MontoMetodoPrimarioExcedido",
                                "El monto del método primario cubre o excede el total. Use un solo método de pago.")));
                }

                var baseImponible = MoneyRounding.Round(subtotal / divisor);
                var impuesto = MoneyRounding.Round(subtotal - baseImponible);

                var transaccion = new Transaccion
                {
                    ClienteId               = request.ClienteId,
                    SedeId                  = request.SedeId,
                    MetodoPagoId            = request.MetodoPagoId,
                    MetodoPagoSecundarioId  = request.MetodoPagoSecundarioId,
                    MontoMetodoPrimario     = request.MontoMetodoPrimario,
                    OpcionEnvioId           = request.OpcionEnvioId,
                    TurnoId                 = request.TurnoId,
                    OperadorId              = request.OperadorId,
                    Canal                   = request.Canal,
                    EsMayorista             = false,
                    Fecha                   = DateTime.UtcNow,
                    Subtotal                = baseImponible,
                    Impuesto                = impuesto,
                    RecargoPropina          = 0m,
                    CostoEnvio              = 0m,
                    Total                   = subtotal,
                    Detalles                = detalles,
                    TipoDocumento           = request.TipoDocumento,
                    NumeroDocumento         = request.NumeroDocumento,
                    RazonSocial             = request.RazonSocial
                };

                await _transacciones.AddAsync(transaccion, token);

                try
                {
                    await _uow.SaveChangesAsync(token);
                }
                catch (CafeBarrio.Application.Common.Exceptions.ConcurrencyException)
                {
                    throw new BusinessFailureException(
                        Result<int>.Failure(new Error("Stock.ConcurrencyConflict", "Conflicto de inventario. Reintenta la venta.")));
                }

                await _idempotencyRecords.AddAsync(new IdempotencyRecord
                {
                    IdempotencyKey = request.IdempotencyKey,
                    TransaccionId  = transaccion.TransaccionId,
                    CreatedAtUtc   = DateTime.UtcNow
                }, token);

                try
                {
                    await _uow.SaveChangesAsync(token);
                }
                catch (CafeBarrio.Application.Common.Exceptions.PersistenceException)
                {
                    var race = await _idempotencyRecords.GetByKeyAsync(request.IdempotencyKey, token);
                    if (race is not null)
                        return race.TransaccionId;
                    throw;
                }

                totalPublicado = subtotal;
                return transaccion.TransaccionId;
            }, ct);
        }
        catch (BusinessFailureException bfe)
        {
            return bfe.Result;
        }

        await _publisher.Publish(new TransaccionCreadaEvent(
            transaccionId, request.SedeId,
            totalPublicado, DateTime.UtcNow), ct);

        return Result<int>.Success(transaccionId);
    }
}
