namespace CafeBarrio.Application.Features.MovimientosCaja.Dtos;

public record MovimientoCajaDto(
    int MovimientoCajaId,
    int TurnoId,
    string TipoMovimiento,
    string Motivo,
    decimal Monto,
    DateTime FechaHora
);
