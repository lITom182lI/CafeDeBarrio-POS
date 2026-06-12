using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Turnos.Queries.GetTurnoActivo;

public class GetTurnoActivoHandler : IRequestHandler<GetTurnoActivoQuery, Result<TurnoActivoDto?>>
{
    private readonly ITurnoRepository _turnos;
    private readonly ICurrentUserService _currentUser;

    public GetTurnoActivoHandler(ITurnoRepository turnos, ICurrentUserService currentUser)
    {
        _turnos = turnos;
        _currentUser = currentUser;
    }

    public async Task<Result<TurnoActivoDto?>> Handle(GetTurnoActivoQuery request, CancellationToken ct)
    {
        if (_currentUser.SedeId is not null && _currentUser.SedeId != request.SedeId)
            return Result<TurnoActivoDto?>.Failure(new Error("Auth.ForbiddenSede",
                "No tienes acceso a esta sede."));

        var turno = await _turnos.GetActivoBySedeAsync(request.SedeId, ct);
        if (turno is null) return Result<TurnoActivoDto?>.Success(null);

        var dto = new TurnoActivoDto(
            turno.TurnoId,
            turno.Operador?.Nombre ?? "Sin operador",
            turno.FechaApertura,
            turno.MontoApertura,
            turno.Estado
        );
        return Result<TurnoActivoDto?>.Success(dto);
    }
}
