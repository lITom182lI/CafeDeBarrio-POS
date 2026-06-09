namespace CafeBarrio.Application.Features.Operadores.Dtos;

public record OperadorResumenDto(int OperadorId, string Nombre, bool Activo);
public record OperadorLoginDto(int OperadorId, string Nombre, string Token);
