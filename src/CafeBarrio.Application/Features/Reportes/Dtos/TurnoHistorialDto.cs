namespace CafeBarrio.Application.Features.Reportes.Dtos;
public record TurnoHistorialDto(int TurnoId, string NombreOperador, DateTime FechaApertura, DateTime? FechaCierre, decimal MontoApertura, decimal? MontoEfectivoCierto, decimal? TotalEfectivoSistema, string Estado);
