namespace CafeBarrio.POS.Dtos;
public record ProductoDto(int ProductoId, string Nombre, decimal Precio,
    decimal CantidadDisponible, string? CategoriaNombre, bool Activo);
