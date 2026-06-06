using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Productos.Commands.UpdateProducto;

public record UpdateProductoCommand(
    int ProductoId,
    string Nombre,
    string? Descripcion,
    decimal Costo,
    decimal Precio,
    decimal CantidadDisponible,
    decimal StockMinimo,
    string UnidadMedida,
    bool SeguimientoInventario,
    bool EsMayorista,
    int CategoriaId,
    bool Activo
) : IRequest<Result>;
