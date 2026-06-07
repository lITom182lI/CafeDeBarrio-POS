using FluentValidation;

namespace CafeBarrio.Application.Features.MovimientosCaja.Commands.CreateMovimientoCaja;

public class CreateMovimientoCajaCommandValidator : AbstractValidator<CreateMovimientoCajaCommand>
{
    private static readonly string[] TiposValidos = { "Entrada", "Salida" };

    public CreateMovimientoCajaCommandValidator()
    {
        RuleFor(x => x.TurnoId).GreaterThan(0);
        RuleFor(x => x.TipoMovimiento).NotEmpty().Must(t => TiposValidos.Contains(t))
            .WithMessage("TipoMovimiento debe ser 'Entrada' o 'Salida'.");
        RuleFor(x => x.Motivo).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Monto).GreaterThan(0);
    }
}
