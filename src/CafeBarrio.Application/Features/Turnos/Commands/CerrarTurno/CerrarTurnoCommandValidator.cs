using FluentValidation;

namespace CafeBarrio.Application.Features.Turnos.Commands.CerrarTurno;

public class CerrarTurnoCommandValidator : AbstractValidator<CerrarTurnoCommand>
{
    public CerrarTurnoCommandValidator()
    {
        RuleFor(x => x.TurnoId).GreaterThan(0);
        RuleFor(x => x.MontoEfectivoCierto).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Observaciones).MaximumLength(500).When(x => x.Observaciones is not null);
    }
}
