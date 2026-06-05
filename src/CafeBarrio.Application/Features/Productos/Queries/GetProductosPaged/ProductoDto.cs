namespace CafeBarrio.Application.Features.Productos.Queries.GetProductosPaged;

public record ProductoDto(
    int ProductoId,
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int CantidadDisponible,
    string CategoriaNombre,
    bool EsMayorista
);
