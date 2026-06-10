using CafeBarrio.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Operadores.Commands.DeleteOperador;

public class DeleteOperadorHandler : IRequestHandler<DeleteOperadorCommand, Result<bool>>
{
    private readonly IOperadorRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<DeleteOperadorHandler> _logger;

    public DeleteOperadorHandler(IOperadorRepository repo, IUnitOfWork uow, ILogger<DeleteOperadorHandler> logger)
    {
        _repo   = repo;
        _uow    = uow;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteOperadorCommand request, CancellationToken ct)
    {
        var operador = await _repo.GetByIdAsync(request.OperadorId, ct);
        if (operador is null)
        {
            _logger.LogWarning("[AUDIT] Intento de eliminación de Operador inexistente. OperadorId={OperadorId}", request.OperadorId);
            return Result<bool>.Failure(new Error("Operador.NotFound", "Operador no encontrado."));
        }

        // Soft delete para no romper relaciones FK con turnos o transacciones históricas
        operador.Activo = false;
        operador.Eliminado = true;

        await _uow.SaveChangesAsync(ct);

        // MUIS_AUDIT Tipo 1 — Registro de acción crítica: eliminación de operador
        _logger.LogWarning("[AUDIT] Operador eliminado (soft-delete). OperadorId={OperadorId} Nombre={Nombre}",
            operador.OperadorId, operador.Nombre);

        return Result<bool>.Success(true);
    }
}
