using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Operadores.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Operadores.Commands.ValidarPin;

public class ValidarPinHandler : IRequestHandler<ValidarPinCommand, Result<OperadorLoginDto>>
{
    private readonly IOperadorRepository _operadores;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwt;
    private readonly IUnitOfWork _uow;

    public ValidarPinHandler(IOperadorRepository operadores, IPasswordHasher hasher, IJwtService jwt, IUnitOfWork uow)
    {
        _operadores = operadores;
        _hasher     = hasher;
        _jwt        = jwt;
        _uow        = uow;
    }

    public async Task<Result<OperadorLoginDto>> Handle(ValidarPinCommand request, CancellationToken ct)
    {
        var operador = await _operadores.GetActivoByIdAsync(request.OperadorId, ct);
        if (operador is null)
            return Result<OperadorLoginDto>.Failure(
                new Error("Operador.NotFound", "Operador no encontrado."));

        // Verificar lockout activo
        if (operador.IsLockedOut && operador.LockedUntilUtc > DateTime.UtcNow)
            return Result<OperadorLoginDto>.Failure(new Error("Operador.Locked",
                $"Operador bloqueado hasta {operador.LockedUntilUtc:HH:mm} UTC."));

        // Resetear lockout expirado
        if (operador.IsLockedOut && operador.LockedUntilUtc <= DateTime.UtcNow)
        {
            operador.IsLockedOut       = false;
            operador.FailedPinAttempts = 0;
            operador.LockedUntilUtc    = null;
        }

        if (!_hasher.Verify(request.Pin, operador.PinHash))
        {
            operador.FailedPinAttempts++;
            if (operador.FailedPinAttempts >= 5)
            {
                operador.IsLockedOut    = true;
                operador.LockedUntilUtc = DateTime.UtcNow.AddMinutes(10);
            }
            await _uow.SaveChangesAsync(ct);
            return Result<OperadorLoginDto>.Failure(
                new Error("Operador.PinInvalido", "PIN incorrecto."));
        }

        // Resetear intentos en éxito
        operador.FailedPinAttempts = 0;
        operador.IsLockedOut       = false;
        operador.LockedUntilUtc    = null;
        await _uow.SaveChangesAsync(ct);

        var token = _jwt.GenerateOperadorToken(
            operador.OperadorId, operador.Nombre,
            operador.SecurityStamp, operador.SedeId);
        return Result<OperadorLoginDto>.Success(
            new OperadorLoginDto(operador.OperadorId, operador.Nombre, token));
    }
}
