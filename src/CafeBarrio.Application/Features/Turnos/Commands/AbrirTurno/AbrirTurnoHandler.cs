using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Turnos.Commands.AbrirTurno;

public class AbrirTurnoHandler : IRequestHandler<AbrirTurnoCommand, Result<int>>
{
    private readonly ITurnoRepository _turnos;
    private readonly IUnitOfWork _uow;

    public AbrirTurnoHandler(ITurnoRepository turnos, IUnitOfWork uow)
    {
        _turnos = turnos;
        _uow = uow;
    }

    public async Task<Result<int>> Handle(AbrirTurnoCommand request, CancellationToken ct)
    {
        var turnoActivo = await _turnos.GetActivoBySedeAsync(request.SedeId, ct);
        if (turnoActivo is not null)
            return Result<int>.Failure(new Error("Turno.YaAbierto", "Ya existe un turno abierto para esta sede."));

        var turno = new Turno
        {
            SedeId = request.SedeId,
            OperadorId = request.OperadorId,
            MontoApertura = request.MontoApertura,
            FechaApertura = DateTime.UtcNow,
            Estado = "Abierto"
        };

        var result = await _turnos.AddAsync(turno, ct);
        if (result.IsFailure) return Result<int>.Failure(result.Errors);

        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(result.Value);
    }
}
