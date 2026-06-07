using FluentValidation;

namespace CafeBarrio.Application.Features.Operadores.Commands.ValidarPin;

public class ValidarPinCommandValidator : AbstractValidator<ValidarPinCommand>
{
    public ValidarPinCommandValidator()
    {
        RuleFor(x => x.OperadorId).GreaterThan(0);
        RuleFor(x => x.Pin).NotEmpty().Length(4, 6).Matches(@"^\d+$")
            .WithMessage("El PIN debe ser numérico de 4 a 6 dígitos.");
    }
}
