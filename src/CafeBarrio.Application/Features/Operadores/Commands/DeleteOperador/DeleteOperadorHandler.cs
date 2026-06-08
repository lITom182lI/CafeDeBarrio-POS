using CafeBarrio.Application.Common.Interfaces;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Operadores.Commands.DeleteOperador;

public class DeleteOperadorHandler : IRequestHandler<DeleteOperadorCommand, Result<bool>>
{
    private readonly IOperadorRepository _repo;
    private readonly IUnitOfWork _uow;

    public DeleteOperadorHandler(IOperadorRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<bool>> Handle(DeleteOperadorCommand request, CancellationToken ct)
    {
        var operador = await _repo.GetByIdAsync(request.OperadorId, ct);
        if (operador is null)
            return Result<bool>.Failure(new Error("Operador.NotFound", "Operador no encontrado."));

        // Soft delete para no romper relaciones FK con turnos o transacciones históricas
        operador.Activo = false;

        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
