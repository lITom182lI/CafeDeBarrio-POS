using FluentValidation;

namespace CafeBarrio.Application.Features.Operadores.Commands.UpdateOperador;

public class UpdateOperadorCommandValidator : AbstractValidator<UpdateOperadorCommand>
{
    public UpdateOperadorCommandValidator()
    {
        RuleFor(x => x.OperadorId).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NuevoPin).Length(4, 6).Matches(@"^\d+$")
            .WithMessage("El PIN debe ser numérico de 4 a 6 dígitos.")
            .When(x => x.NuevoPin is not null);
    }
}
