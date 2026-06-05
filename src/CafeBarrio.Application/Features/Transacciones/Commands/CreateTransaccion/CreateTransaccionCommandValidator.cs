using FluentValidation;

namespace CafeBarrio.Application.Features.Transacciones.Commands.CreateTransaccion;

public class CreateTransaccionCommandValidator : AbstractValidator<CreateTransaccionCommand>
{
    public CreateTransaccionCommandValidator()
    {
        RuleFor(x => x.ClienteId).GreaterThan(0);
        RuleFor(x => x.SedeId).GreaterThan(0);
        RuleFor(x => x.MetodoPagoId).GreaterThan(0);
        RuleFor(x => x.Canal).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductoId).GreaterThan(0);
            item.RuleFor(i => i.Cantidad).GreaterThan(0);
        });
    }
}
