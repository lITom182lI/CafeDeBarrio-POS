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
    private readonly ISunatService _sunat;
    private readonly IPublisher _publisher;

    public CreateTransaccionHandler(
        ITransaccionRepository transacciones,
        IProductoRepository productos,
        IConfiguracionNegocioRepository configuracion,
        IUnitOfWork uow,
        ISunatService sunat,
        IPublisher publisher)
    {
        _transacciones = transacciones;
        _productos     = productos;
        _configuracion = configuracion;
        _uow           = uow;
        _sunat         = sunat;
        _publisher     = publisher;
    }

    public async Task<Result<int>> Handle(CreateTransaccionCommand request, CancellationToken ct)
    {
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

            if (producto.SeguimientoInventario && producto.CantidadDisponible < item.Cantidad)
                return Result<int>.Failure(new Error("Producto.StockInsuficiente",
                    $"Insufficient stock for product {item.ProductoId}."));

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

            if (producto.SeguimientoInventario)
                producto.CantidadDisponible -= item.Cantidad;
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
            RazonSocial    = request.RazonSocial
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

        try
        {
            var boletaItems = detalles.Select(d => new BoletaItem(
                nombres.GetValueOrDefault(d.ProductoId, $"P{d.ProductoId}"),
                d.Cantidad,
                d.PrecioUnitario,
                d.SubtotalLinea)).ToList();

            var sunatResult = await _sunat.EmitirBoletaAsync(new EmitirBoletaRequest(
                transaccion.TransaccionId, transaccion.Fecha,
                boletaItems, subtotal, impuesto, transaccion.Total, request.Canal,
                request.TipoDocumento, request.NumeroDocumento, request.RazonSocial), ct);

            transaccion.SunatEstado      = sunatResult.Emitida ? "Emitida" : "NoEmitida";
            transaccion.SunatNumeroSerie = sunatResult.NumeroSerie;
            transaccion.SunatError       = sunatResult.Emitida ? null : (sunatResult.Error ?? sunatResult.Mensaje);
        }
        catch (Exception ex)
        {
            transaccion.SunatEstado = "Fallida";
            transaccion.SunatError  = ex.Message;
        }
        finally
        {
            await _uow.SaveChangesAsync(ct);
        }

        await _publisher.Publish(new TransaccionCreadaEvent(
            transaccion.TransaccionId, transaccion.SedeId,
            transaccion.Total, transaccion.Fecha), ct);
        return Result<int>.Success(transaccion.TransaccionId);
    }
}
