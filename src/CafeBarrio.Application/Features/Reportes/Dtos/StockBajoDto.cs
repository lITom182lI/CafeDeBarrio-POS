namespace CafeBarrio.Application.Features.Reportes.Dtos;
public record StockBajoDto(int ProductoId, string Nombre, decimal CantidadDisponible, decimal StockMinimo, string UnidadMedida);
