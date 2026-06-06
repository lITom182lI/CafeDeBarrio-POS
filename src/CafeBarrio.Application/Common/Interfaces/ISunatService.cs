namespace CafeBarrio.Application.Common.Interfaces;

public interface ISunatService
{
    Task<EmitirBoletaResult> EmitirBoletaAsync(EmitirBoletaRequest request, CancellationToken ct = default);
}

public record BoletaItem(
    string Nombre,
    int Cantidad,
    decimal PrecioUnitario,
    decimal SubtotalLinea
);

public record EmitirBoletaRequest(
    int TransaccionId,
    DateTime Fecha,
    List<BoletaItem> Items,
    decimal Subtotal,
    decimal Impuesto,
    decimal Total,
    string Canal
);

public record EmitirBoletaResult(
    bool Emitida,
    string? NumeroSerie,
    string Mensaje,
    string? Error = null
);
