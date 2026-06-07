using FluentValidation;

namespace CafeBarrio.Application.Features.Productos.Commands.CreateProducto;

public class CreateProductoCommandValidator : AbstractValidator<CreateProductoCommand>
{
    public CreateProductoCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Costo).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Precio).GreaterThan(0);
        RuleFor(x => x.CantidadDisponible).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StockMinimo).GreaterThanOrEqualTo(0);
        RuleFor(x => x.UnidadMedida).NotEmpty().MaximumLength(20);
        RuleFor(x => x.CategoriaId).GreaterThan(0);
    }
}
