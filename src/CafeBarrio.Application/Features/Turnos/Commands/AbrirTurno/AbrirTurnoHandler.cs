using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Turnos.Commands.AbrirTurno;

public class AbrirTurnoHandler : IRequestHandler<AbrirTurnoCommand, Result<int>>
{
    private readonly ITurnoRepository _turnos;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public AbrirTurnoHandler(ITurnoRepository turnos, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _turnos = turnos;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(AbrirTurnoCommand request, CancellationToken ct)
    {
        if (_currentUser.SedeId is not null && _currentUser.SedeId != request.SedeId)
            return Result<int>.Failure(new Error("Auth.ForbiddenSede",
                "No tienes acceso a esta sede."));

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

        await _turnos.AddAsync(turno, ct);

        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(turno.TurnoId);
    }
}
