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

        var resumen = await _turnos.GetResumenEfectivoAsync(request.TurnoId, ct);

        var diferencia = request.MontoEfectivoCierto - resumen.SaldoEsperado;

        turno.FechaCierre = DateTime.UtcNow;
        turno.MontoEfectivoCierto = request.MontoEfectivoCierto;
        turno.TotalEfectivoSistema = resumen.SaldoEsperado; // Maintained for backwards compatibility in other parts of the system if needed
        turno.Estado = "Cerrado";
        turno.Observaciones = request.Observaciones;

        // Guardar desglose en el turno cerrado
        turno.TotalVentasEfectivo      = resumen.TotalVentasEfectivo;
        turno.TotalAnulacionesEfectivo = resumen.TotalAnulacionesEfectivo;
        turno.TotalMovimientosEntrada  = resumen.TotalEntradasCaja;
        turno.TotalMovimientosSalida   = resumen.TotalSalidasCaja;
        turno.SaldoEsperado            = resumen.SaldoEsperado;
        turno.Diferencia               = diferencia;

        await _uow.SaveChangesAsync(ct);

        var dto = new CerrarTurnoResultDto(resumen.SaldoEsperado, request.MontoEfectivoCierto, diferencia);
        return Result<CerrarTurnoResultDto>.Success(dto);
    }
}
