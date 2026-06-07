using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.MovimientosCaja.Dtos;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.MovimientosCaja.Queries.GetMovimientosByTurno;

public class GetMovimientosByTurnoHandler : IRequestHandler<GetMovimientosByTurnoQuery, Result<IReadOnlyList<MovimientoCajaDto>>>
{
    private readonly IMovimientoCajaRepository _movimientosCaja;
    private readonly ITurnoRepository _turnos;

    public GetMovimientosByTurnoHandler(IMovimientoCajaRepository movimientosCaja, ITurnoRepository turnos)
    {
        _movimientosCaja = movimientosCaja;
        _turnos = turnos;
    }

    public async Task<Result<IReadOnlyList<MovimientoCajaDto>>> Handle(GetMovimientosByTurnoQuery request, CancellationToken ct)
    {
        var turno = await _turnos.GetByIdAsync(request.TurnoId, ct);
        if (turno is null)
            return Result<IReadOnlyList<MovimientoCajaDto>>.Failure(new Error("MovimientoCaja.TurnoNotFound", "El turno especificado no existe."));

        var movimientos = await _movimientosCaja.GetByTurnoAsync(request.TurnoId, ct);

        var dtos = movimientos.Select(m => new MovimientoCajaDto(
            m.MovimientoCajaId,
            m.TurnoId,
            m.TipoMovimiento,
            m.Motivo,
            m.Monto,
            m.FechaHora
        )).ToList();

        return Result<IReadOnlyList<MovimientoCajaDto>>.Success(dtos);
    }
}
