using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.MovimientosCaja.Commands.CreateMovimientoCaja;

public class CreateMovimientoCajaHandler : IRequestHandler<CreateMovimientoCajaCommand, Result<int>>
{
    private readonly IMovimientoCajaRepository _movimientosCaja;
    private readonly ITurnoRepository _turnos;
    private readonly IUnitOfWork _uow;

    public CreateMovimientoCajaHandler(
        IMovimientoCajaRepository movimientosCaja,
        ITurnoRepository turnos,
        IUnitOfWork uow)
    {
        _movimientosCaja = movimientosCaja;
        _turnos = turnos;
        _uow = uow;
    }

    public async Task<Result<int>> Handle(CreateMovimientoCajaCommand request, CancellationToken ct)
    {
        if (request.TipoMovimiento != "Ingreso" && request.TipoMovimiento != "Egreso")
            return Result<int>.Failure(new Error("MovimientoCaja.TipoInvalido", "El tipo de movimiento debe ser 'Ingreso' o 'Egreso'."));

        if (request.Monto <= 0)
            return Result<int>.Failure(new Error("MovimientoCaja.MontoInvalido", "El monto debe ser mayor a 0."));

        var turno = await _turnos.GetByIdAsync(request.TurnoId, ct);
        if (turno is null)
            return Result<int>.Failure(new Error("MovimientoCaja.TurnoNotFound", "El turno especificado no existe."));

        if (turno.Estado != "Abierto")
            return Result<int>.Failure(new Error("MovimientoCaja.TurnoCerrado", "El turno especificado está cerrado."));

        var movimiento = new MovimientoCaja
        {
            TurnoId = request.TurnoId,
            TipoMovimiento = request.TipoMovimiento,
            Motivo = request.Motivo,
            Monto = request.Monto,
            FechaHora = DateTime.UtcNow
        };

        await _movimientosCaja.AddAsync(movimiento, ct);

        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(movimiento.MovimientoCajaId);
    }
}
