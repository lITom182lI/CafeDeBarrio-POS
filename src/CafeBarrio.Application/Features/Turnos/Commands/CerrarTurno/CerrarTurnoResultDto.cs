namespace CafeBarrio.Application.Features.Turnos.Commands.CerrarTurno;

public record CerrarTurnoResultDto(
    decimal TotalEfectivoSistema,
    decimal MontoEfectivoCierto,
    decimal Diferencia
);
