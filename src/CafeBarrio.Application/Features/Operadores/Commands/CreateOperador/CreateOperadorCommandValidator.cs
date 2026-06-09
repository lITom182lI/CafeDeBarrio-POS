using FluentValidation;

namespace CafeBarrio.Application.Features.Operadores.Commands.CreateOperador;

public class CreateOperadorCommandValidator : AbstractValidator<CreateOperadorCommand>
{
    public CreateOperadorCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Pin).NotEmpty().Length(4, 8).Matches(@"^\d+$")
            .WithMessage("El PIN debe ser numérico de 4 a 8 dígitos.");
    }
}
