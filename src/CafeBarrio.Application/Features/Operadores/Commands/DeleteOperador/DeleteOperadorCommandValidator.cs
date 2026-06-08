using FluentValidation;

namespace CafeBarrio.Application.Features.Operadores.Commands.DeleteOperador;

public class DeleteOperadorCommandValidator : AbstractValidator<DeleteOperadorCommand>
{
    public DeleteOperadorCommandValidator()
    {
        RuleFor(x => x.OperadorId).GreaterThan(0);
    }
}
