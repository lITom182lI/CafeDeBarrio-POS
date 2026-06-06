namespace CafeBarrio.Application.Features.Reportes.Dtos;
public record AnulacionResumenDto(int AnulacionId, int TransaccionId, string TipoAnulacion, string Motivo, decimal MontoOriginal, decimal MontoDevuelto, DateTime FechaHora);
