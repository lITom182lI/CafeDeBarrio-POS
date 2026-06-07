using FluentValidation;

namespace CafeBarrio.Application.Features.Turnos.Commands.AbrirTurno;

public class AbrirTurnoCommandValidator : AbstractValidator<AbrirTurnoCommand>
{
    public AbrirTurnoCommandValidator()
    {
        RuleFor(x => x.SedeId).GreaterThan(0);
        RuleFor(x => x.OperadorId).GreaterThan(0);
        RuleFor(x => x.MontoApertura).GreaterThanOrEqualTo(0);
    }
}
