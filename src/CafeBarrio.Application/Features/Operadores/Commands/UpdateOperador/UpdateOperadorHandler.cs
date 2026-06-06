using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Operadores.Commands.UpdateOperador;

public class UpdateOperadorHandler : IRequestHandler<UpdateOperadorCommand, Result>
{
    private readonly IOperadorRepository _operadores;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;

    public UpdateOperadorHandler(IOperadorRepository operadores, IUnitOfWork uow, IPasswordHasher hasher)
    {
        _operadores = operadores;
        _uow        = uow;
        _hasher     = hasher;
    }

    public async Task<Result> Handle(UpdateOperadorCommand r, CancellationToken ct)
    {
        var operador = await _operadores.GetByIdAsync(r.OperadorId, ct);
        if (operador is null)
            return Result.Failure(new Error("Operador.NotFound",
                $"Operador {r.OperadorId} no encontrado."));

        if (r.NuevoPin is not null)
        {
            if (r.NuevoPin.Length < 4 || r.NuevoPin.Length > 8 || !r.NuevoPin.All(char.IsDigit))
                return Result.Failure(new Error("Operador.PinInvalido",
                    "El PIN debe tener entre 4 y 8 dígitos numéricos."));
            operador.PinHash = _hasher.Hash(r.NuevoPin);
        }

        operador.Nombre = r.Nombre.Trim();
        operador.Activo = r.Activo;
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
