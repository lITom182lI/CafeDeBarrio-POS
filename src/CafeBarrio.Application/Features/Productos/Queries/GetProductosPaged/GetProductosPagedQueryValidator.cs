using FluentValidation;

namespace CafeBarrio.Application.Features.Productos.Queries.GetProductosPaged;

public class GetProductosPagedQueryValidator : AbstractValidator<GetProductosPagedQuery>
{
    public GetProductosPagedQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber debe ser mayor a 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize debe ser mayor a 0.")
            .LessThanOrEqualTo(2000).WithMessage("PageSize no puede superar 2000.");
    }
}
