using FluentValidation;

namespace CafeBarrio.Application.Features.Anulaciones.Commands.CreateAnulacion;

public class CreateAnulacionCommandValidator : AbstractValidator<CreateAnulacionCommand>
{
    private static readonly string[] TiposValidos = { "Total", "Parcial" };

    public CreateAnulacionCommandValidator()
    {
        RuleFor(x => x.TransaccionId).GreaterThan(0);
        RuleFor(x => x.TipoAnulacion).NotEmpty().Must(t => TiposValidos.Contains(t))
            .WithMessage("TipoAnulacion debe ser 'Total' o 'Parcial'.");
        RuleFor(x => x.Motivo).NotEmpty().MaximumLength(500);
        RuleFor(x => x.MontoDevuelto).GreaterThan(0);
        RuleFor(x => x.OperadorSolicitanteId).GreaterThan(0);
        RuleFor(x => x.AutorizadorId).GreaterThan(0);
    }
}
