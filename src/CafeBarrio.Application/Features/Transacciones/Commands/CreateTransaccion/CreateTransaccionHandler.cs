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

    public CreateTransaccionHandler(
        ITransaccionRepository transacciones,
        IProductoRepository productos,
        IConfiguracionNegocioRepository configuracion,
        IUnitOfWork uow,
        ISunatService sunat)
    {
        _transacciones = transacciones;
        _productos     = productos;
        _configuracion = configuracion;
        _uow           = uow;
        _sunat         = sunat;
    }

    public async Task<Result<int>> Handle(CreateTransaccionCommand request, CancellationToken ct)
    {
        var config  = await _configuracion.GetActivaBySedeAsync(request.SedeId, ct);
        var tasaIgv = config is not null ? config.TasaIGV + config.TasaIPM : 0.105m;

        var detalles = new List<DetalleTransaccion>();
        decimal subtotal = 0;

        foreach (var item in request.Items)
        {
            var producto = await _productos.GetByIdAsync(item.ProductoId, ct);
            if (producto is null)
                return Result<int>.Failure(new Error("Producto.NotFound",
                    $"Producto {item.ProductoId} no encontrado."));

            if (producto.SeguimientoInventario && producto.CantidadDisponible < item.Cantidad)
                return Result<int>.Failure(new Error("Producto.StockInsuficiente",
                    $"Insufficient stock for product {item.ProductoId}."));

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

        var result = await _transacciones.AddAsync(transaccion, ct);
        if (result.IsFailure) return Result<int>.Failure(result.Errors);

        await _uow.SaveChangesAsync(ct);

        // Emisión SUNAT — no bloquea la venta si falla
        try
        {
            var boletaItems = detalles.Select(d => new BoletaItem(
                d.ProductoId.ToString(),
                d.Cantidad,
                d.PrecioUnitario,
                d.SubtotalLinea)).ToList();

            await _sunat.EmitirBoletaAsync(new EmitirBoletaRequest(
                result.Value, transaccion.Fecha,
                boletaItems, subtotal, impuesto,
                transaccion.Total, request.Canal), ct);
        }
        catch { /* venta ya persistida — error SUNAT no la revierte */ }

        return Result<int>.Success(result.Value);
    }
}
