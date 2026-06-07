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

    public ValidarPinHandler(IOperadorRepository operadores, IPasswordHasher hasher, IJwtService jwt)
    {
        _operadores = operadores;
        _hasher     = hasher;
        _jwt        = jwt;
    }

    public async Task<Result<OperadorLoginDto>> Handle(ValidarPinCommand request, CancellationToken ct)
    {
        var operador = await _operadores.GetActivoByIdAsync(request.OperadorId, ct);
        if (operador is null)
            return Result<OperadorLoginDto>.Failure(
                new Error("Operador.NotFound", "Operador no encontrado."));

        if (!_hasher.Verify(request.Pin, operador.PinHash))
            return Result<OperadorLoginDto>.Failure(
                new Error("Operador.PinInvalido", "PIN incorrecto."));

        var token = _jwt.GenerateOperadorToken(operador.OperadorId, operador.Nombre);
        return Result<OperadorLoginDto>.Success(
            new OperadorLoginDto(operador.OperadorId, operador.Nombre, token));
    }
}
