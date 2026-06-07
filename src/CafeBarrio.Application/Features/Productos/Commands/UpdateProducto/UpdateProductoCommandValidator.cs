using FluentValidation;

namespace CafeBarrio.Application.Features.Productos.Commands.UpdateProducto;

public class UpdateProductoCommandValidator : AbstractValidator<UpdateProductoCommand>
{
    public UpdateProductoCommandValidator()
    {
        RuleFor(x => x.ProductoId).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Costo).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Precio).GreaterThan(0);
        RuleFor(x => x.CantidadDisponible).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StockMinimo).GreaterThanOrEqualTo(0);
        RuleFor(x => x.UnidadMedida).NotEmpty().MaximumLength(20);
        RuleFor(x => x.CategoriaId).GreaterThan(0);
    }
}
