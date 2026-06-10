namespace CafeBarrio.Application.Features.Reportes.Dtos;

public record TurnoCerradoDto(
    int TurnoId,
    int OperadorId,
    string OperadorNombre,
    DateTime FechaApertura,
    DateTime FechaCierre,
    decimal MontoApertura,
    decimal MontoEfectivoCierto,
    decimal TotalEfectivoSistema,
    decimal Diferencia,
    string EstadoCierre,
    string? Observaciones);
