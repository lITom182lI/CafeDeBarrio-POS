using CafeBarrio.Application.Common.Interfaces;
using MediatR;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Application.Features.Turnos.Commands.CerrarTurno;

public class CerrarTurnoHandler : IRequestHandler<CerrarTurnoCommand, Result<CerrarTurnoResultDto>>
{
    private readonly ITurnoRepository _turnos;
    private readonly IUnitOfWork _uow;

    public CerrarTurnoHandler(ITurnoRepository turnos, IUnitOfWork uow)
    {
        _turnos = turnos;
        _uow = uow;
    }

    public async Task<Result<CerrarTurnoResultDto>> Handle(CerrarTurnoCommand request, CancellationToken ct)
    {
        var turno = await _turnos.GetByIdAsync(request.TurnoId, ct);
        if (turno is null)
            return Result<CerrarTurnoResultDto>.Failure(new Error("Turno.NotFound", "Turno no encontrado."));
        if (turno.Estado != "Abierto")
            return Result<CerrarTurnoResultDto>.Failure(new Error("Turno.NoPuedesCerrar", "El turno no está en estado Abierto."));

        var totalEfectivo = await _turnos.GetTotalEfectivoByTurnoAsync(request.TurnoId, ct);

        turno.FechaCierre = DateTime.UtcNow;
        turno.MontoEfectivoCierto = request.MontoEfectivoCierto;
        turno.TotalEfectivoSistema = turno.MontoApertura + totalEfectivo;
        turno.Estado = "Cerrado";
        turno.Observaciones = request.Observaciones;

        await _uow.SaveChangesAsync(ct);

        var diferencia = request.MontoEfectivoCierto - turno.TotalEfectivoSistema.Value;
        var dto = new CerrarTurnoResultDto(turno.TotalEfectivoSistema.Value, request.MontoEfectivoCierto, diferencia);
        return Result<CerrarTurnoResultDto>.Success(dto);
    }
}
