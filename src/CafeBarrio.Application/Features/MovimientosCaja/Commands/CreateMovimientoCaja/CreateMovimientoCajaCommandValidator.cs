using CafeBarrio.Domain.Common;
using FluentValidation;

namespace CafeBarrio.Application.Features.MovimientosCaja.Commands.CreateMovimientoCaja;

public class CreateMovimientoCajaCommandValidator : AbstractValidator<CreateMovimientoCajaCommand>
{
    private static readonly string[] TiposValidos = { TipoMovimiento.Entrada, TipoMovimiento.Salida };

    public CreateMovimientoCajaCommandValidator()
    {
        RuleFor(x => x.TurnoId).GreaterThan(0);
        RuleFor(x => x.TipoMovimiento).NotEmpty().Must(t => t == TipoMovimiento.Entrada || t == TipoMovimiento.Salida)
            .WithMessage($"TipoMovimiento debe ser '{TipoMovimiento.Entrada}' o '{TipoMovimiento.Salida}'.");
        RuleFor(x => x.Motivo).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Monto).GreaterThan(0);
    }
}
