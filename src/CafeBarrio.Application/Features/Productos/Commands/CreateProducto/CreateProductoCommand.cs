using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Productos.Commands.CreateProducto;

public record CreateProductoCommand(
    string Nombre,
    string? Descripcion,
    decimal Costo,
    decimal Precio,
    decimal CantidadDisponible,
    decimal StockMinimo,
    string UnidadMedida,
    bool SeguimientoInventario,
    bool EsMayorista,
    int CategoriaId
) : IRequest<Result<int>>;
