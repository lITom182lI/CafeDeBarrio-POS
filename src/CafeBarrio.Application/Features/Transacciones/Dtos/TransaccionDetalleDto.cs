namespace CafeBarrio.Application.Features.Transacciones.Dtos;

public record TransaccionDetalleDto(
    int TransaccionId,
    string ClienteNombre,
    decimal Total,
    decimal Subtotal,
    decimal Igv,
    DateTime Fecha,
    string MetodoPago,
    bool Anulada,
    IReadOnlyList<DetalleItemDto> Items,
    string? OperadorNombre = null,
    string? TipoDocumento = null,
    string? NumeroDocumento = null,
    string? RazonSocial = null,
    string? MetodoPagoSecundario = null,
    decimal? MontoMetodoPrimario = null);


public record DetalleItemDto(
    string NombreProducto,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal SubtotalLinea);
