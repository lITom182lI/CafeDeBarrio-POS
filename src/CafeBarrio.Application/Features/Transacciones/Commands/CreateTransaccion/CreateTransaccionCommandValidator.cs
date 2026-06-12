using FluentValidation;
using CafeBarrio.Domain.ValueObjects;

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
        RuleFor(x => x)
            .Custom((cmd, ctx) =>
            {
                try { ComprobanteData.TryCreate(cmd.TipoDocumento, cmd.NumeroDocumento, cmd.RazonSocial); }
                catch (ArgumentException ex) { ctx.AddFailure(ex.Message); }
            })
            .When(x => x.TipoDocumento is not null || x.NumeroDocumento is not null);

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .MaximumLength(64);

        // Pago dividido: el método secundario no puede ser igual al primario
        RuleFor(x => x.MetodoPagoSecundarioId)
            .NotEqual(x => (int?)x.MetodoPagoId)
            .When(x => x.MetodoPagoSecundarioId.HasValue)
            .WithMessage("El método de pago secundario no puede ser igual al método primario.");

        // Pago dividido: MontoMetodoPrimario es obligatorio y debe ser positivo
        RuleFor(x => x.MontoMetodoPrimario)
            .NotNull()
            .GreaterThan(0m)
            .When(x => x.MetodoPagoSecundarioId.HasValue)
            .WithMessage("Se requiere el monto del método primario para un pago dividido.");
    }
}
