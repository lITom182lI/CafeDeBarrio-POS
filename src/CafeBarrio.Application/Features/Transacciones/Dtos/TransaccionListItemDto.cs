namespace CafeBarrio.Application.Features.Transacciones.Dtos;

public record TransaccionListItemDto(
    int TransaccionId,
    string ClienteNombre,
    decimal Total,
    DateTime Fecha,
    string MetodoPago,
    bool Anulada,
    string? OperadorNombre = null,
    string? TipoDocumento = null,
    string? NumeroDocumento = null,
    string? RazonSocial = null,
    string? MetodoPagoSecundario = null,
    string? MotivoAnulacion = null);

