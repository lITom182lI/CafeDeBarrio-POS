namespace CafeBarrio.Application.Features.Operadores.Dtos;

public record OperadorResumenDto(int OperadorId, string Nombre, bool Activo, bool Eliminado = false);
public record OperadorLoginDto(int OperadorId, string Nombre, string Token);
public record OperadorLoginItemDto(int OperadorId, string Nombre);
