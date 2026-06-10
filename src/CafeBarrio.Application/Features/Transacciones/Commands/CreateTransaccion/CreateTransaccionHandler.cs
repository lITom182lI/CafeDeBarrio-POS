using CafeBarrio.Application.Events;
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

    public CreateTransaccionHandler(
        ITransaccionRepository transacciones,
        IProductoRepository productos,
        IConfiguracionNegocioRepository configuracion,
        IUnitOfWork uow,
        IPublisher publisher)
    {
        _transacciones = transacciones;
        _productos     = productos;
        _configuracion = configuracion;
        _uow           = uow;
        _publisher     = publisher;
    }

    public async Task<Result<int>> Handle(CreateTransaccionCommand request, CancellationToken ct)
    {
        if (request.IdempotencyKey.HasValue)
        {
            var existente = await _transacciones.GetByIdempotencyKeyAsync(request.IdempotencyKey.Value, ct);
            if (existente is not null)
                return Result<int>.Success(existente.TransaccionId);
        }

        var config  = await _configuracion.GetActivaBySedeAsync(request.SedeId, ct);
        if (config is null)
            return Result<int>.Failure(new Error("Configuracion.NotFound", "No se encontró configuración activa para la sede."));

        var tasaIgv = config.TasaIGV + config.TasaIPM;
        var detalles = new List<DetalleTransaccion>();
        decimal subtotal = 0;
        var nombres = new Dictionary<int, string>();

        foreach (var item in request.Items)
        {
            var producto = await _productos.GetByIdAsync(item.ProductoId, ct);
            if (producto is null)
                return Result<int>.Failure(new Error("Producto.NotFound",
                    $"Producto {item.ProductoId} no encontrado."));

            var descuento = producto.DescontarStock(item.Cantidad);
            if (descuento.IsFailure)
                return Result<int>.Failure(descuento.Errors[0]);

            nombres[item.ProductoId] = producto.Nombre;

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

        var impuesto = Math.Round(subtotal * tasaIgv, 2);

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
            RazonSocial    = request.RazonSocial,
            IdempotencyKey = request.IdempotencyKey
        };

        await _transacciones.AddAsync(transaccion, ct);

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (CafeBarrio.Application.Common.Exceptions.ConcurrencyException)
        {
            return Result<int>.Failure(new Error("Stock.ConcurrencyConflict", "Conflicto de inventario. Reintenta la venta."));
        }

        await _publisher.Publish(new TransaccionCreadaEvent(
            transaccion.TransaccionId, transaccion.SedeId,
            transaccion.Total, transaccion.Fecha), ct);
        return Result<int>.Success(transaccion.TransaccionId);
    }
}
