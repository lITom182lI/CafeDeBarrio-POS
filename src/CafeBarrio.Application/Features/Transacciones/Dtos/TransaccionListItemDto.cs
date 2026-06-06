namespace CafeBarrio.Application.Features.Transacciones.Dtos;

public record TransaccionListItemDto(
    int TransaccionId,
    string ClienteNombre,
    decimal Total,
    DateTime Fecha,
    string MetodoPago,
    bool Anulada);
