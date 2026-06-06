using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Operadores.Commands.CreateOperador;

public class CreateOperadorHandler : IRequestHandler<CreateOperadorCommand, Result<int>>
{
    private readonly IOperadorRepository _operadores;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;

    public CreateOperadorHandler(IOperadorRepository operadores, IUnitOfWork uow, IPasswordHasher hasher)
    {
        _operadores = operadores;
        _uow        = uow;
        _hasher     = hasher;
    }

    public async Task<Result<int>> Handle(CreateOperadorCommand r, CancellationToken ct)
    {
        if (r.Pin.Length < 4 || r.Pin.Length > 8 || !r.Pin.All(char.IsDigit))
            return Result<int>.Failure(new Error("Operador.PinInvalido",
                "El PIN debe tener entre 4 y 8 dígitos numéricos."));

        var operador = new Operador
        {
            SedeId  = 1,
            Nombre  = r.Nombre.Trim(),
            PinHash = _hasher.Hash(r.Pin),
            Activo  = true
        };
        
        var result = await _operadores.AddAsync(operador, ct);
        if (result.IsFailure) return Result<int>.Failure(result.Errors);
        
        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(result.Value);
    }
}
