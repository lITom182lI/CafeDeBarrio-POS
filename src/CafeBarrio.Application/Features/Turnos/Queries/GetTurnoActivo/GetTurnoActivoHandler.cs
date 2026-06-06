using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Reportes.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Turnos.Queries.GetTurnoActivo;

public class GetTurnoActivoHandler : IRequestHandler<GetTurnoActivoQuery, Result<TurnoActivoDto?>>
{
    private readonly ITurnoRepository _turnos;
    public GetTurnoActivoHandler(ITurnoRepository turnos) => _turnos = turnos;

    public async Task<Result<TurnoActivoDto?>> Handle(GetTurnoActivoQuery request, CancellationToken ct)
    {
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
