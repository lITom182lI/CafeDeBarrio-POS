using FluentValidation;

namespace CafeBarrio.Application.Features.Operadores.Commands.ValidarPin;

public class ValidarPinCommandValidator : AbstractValidator<ValidarPinCommand>
{
    public ValidarPinCommandValidator()
    {
        RuleFor(x => x.OperadorId).GreaterThan(0);
        RuleFor(x => x.Pin).NotEmpty().Length(6, 8).Matches(@"^\d+$")
            .WithMessage("El PIN debe ser numérico de 6 a 8 dígitos.");
    }
}
