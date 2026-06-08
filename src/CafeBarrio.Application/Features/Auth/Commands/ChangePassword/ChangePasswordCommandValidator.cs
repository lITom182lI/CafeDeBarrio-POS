using FluentValidation;

namespace CafeBarrio.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
            .WithMessage("La nueva contraseña debe tener al menos 8 caracteres.");
    }
}
