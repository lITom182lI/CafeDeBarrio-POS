namespace CafeBarrio.Infrastructure.External.Sunat;

public interface ISunatOseApiClient
{
    Task<OseEmisionResult> EmitirBoletaAsync(OseBoletaRequest request, CancellationToken ct);
}

public sealed record OseBoletaRequest(
    string         Serie,
    int            TipoDocCliente,
    string         NumDocCliente,
    string         DenominacionCliente,
    DateTime       Fecha,
    decimal        PorcentajeIgv,
    List<OseBoletaItem> Items,
    decimal        TotalGravadas,
    decimal        TotalIgv,
    decimal        Total);

public sealed record OseBoletaItem(
    string  Codigo,
    string  Descripcion,
    int     Cantidad,
    decimal ValorUnitario,
    decimal PrecioUnitario,
    decimal Subtotal,
    decimal Igv,
    decimal Total);

public sealed record OseEmisionResult(
    bool    Aceptada,
    string? NumeroSerie,
    string? Mensaje,
    string? Error,
    bool    Retryable = false);
