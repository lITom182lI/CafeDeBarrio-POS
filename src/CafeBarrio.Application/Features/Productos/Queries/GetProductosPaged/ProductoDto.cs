namespace CafeBarrio.Application.Features.Productos.Queries.GetProductosPaged;

public record ProductoDto(
    int ProductoId,
    string Nombre,
    string? Descripcion,
    decimal Precio,
    decimal CantidadDisponible,
    decimal StockMinimo,
    string UnidadMedida,
    int CategoriaId,
    string CategoriaNombre,
    bool EsMayorista,
    bool Activo
);
