using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Transacciones.Commands.CreateTransaccion;

public class CreateTransaccionHandler : IRequestHandler<CreateTransaccionCommand, Result<int>>
{
    private readonly ITransaccionRepository _transacciones;
    private readonly IProductoRepository _productos;
    private readonly IUnitOfWork _uow;

    public CreateTransaccionHandler(
        ITransaccionRepository transacciones,
        IProductoRepository productos,
        IUnitOfWork uow)
    {
        _transacciones = transacciones;
        _productos = productos;
        _uow = uow;
    }

    public async Task<Result<int>> Handle(CreateTransaccionCommand request, CancellationToken ct)
    {
        var detalles = new List<DetalleTransaccion>();
        decimal subtotal = 0;

        foreach (var item in request.Items)
        {
            var producto = await _productos.GetByIdAsync(item.ProductoId, ct);
            if (producto is null)
                return Result<int>.Failure(new Error("Producto.NotFound", $"Producto {item.ProductoId} no encontrado."));

            var linea = new DetalleTransaccion
            {
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad,
                PrecioUnitario = producto.Precio,
                SubtotalLinea = producto.Precio * item.Cantidad
            };
            detalles.Add(linea);
            subtotal += linea.SubtotalLinea;
        }

        const decimal tasaIgv = 0.18m;
        var impuesto = Math.Round(subtotal * tasaIgv, 2);

        var transaccion = new Transaccion
        {
            ClienteId = request.ClienteId,
            SedeId = request.SedeId,
            MetodoPagoId = request.MetodoPagoId,
            OpcionEnvioId = request.OpcionEnvioId,
            Canal = request.Canal,
            EsMayorista = false,
            Fecha = DateTime.UtcNow,
            Subtotal = subtotal,
            Impuesto = impuesto,
            CostoEnvio = 0,
            Total = subtotal + impuesto,
            Detalles = detalles
        };

        var result = await _transacciones.AddAsync(transaccion, ct);
        if (result.IsFailure) return Result<int>.Failure(result.Errors);

        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(result.Value);
    }
}
