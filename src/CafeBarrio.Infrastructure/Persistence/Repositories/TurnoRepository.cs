using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Reportes.Dtos;
using CafeBarrio.Application.Features.Turnos.Dtos;
using CafeBarrio.Domain.Entities;
using CafeBarrio.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace CafeBarrio.Infrastructure.Persistence.Repositories;

public class TurnoRepository : BaseRepository<Turno>, ITurnoRepository
{
    public TurnoRepository(CafeBarrioDbContext context) : base(context) { }

    public async Task<Turno?> GetActivoBySedeAsync(int sedeId, CancellationToken ct = default)
        => await Context.Set<Turno>()
               .Include(t => t.Operador)
               .FirstOrDefaultAsync(t => t.SedeId == sedeId && t.Estado == "Abierto", ct);

    public async Task<IReadOnlyList<TurnoHistorialDto>> GetHistorialBySedeAsync(int sedeId, int top, CancellationToken ct = default)
        => await Context.Set<Turno>()
               .Where(t => t.SedeId == sedeId && t.Estado != "Abierto")
               .OrderByDescending(t => t.FechaCierre)
               .Take(top)
               .Select(t => new TurnoHistorialDto(
                   t.TurnoId, t.Operador.Nombre, t.FechaApertura, t.FechaCierre,
                   t.MontoApertura, t.MontoEfectivoCierto, t.TotalEfectivoSistema, t.Estado))
               .ToListAsync(ct);

    public async Task<ResumenEfectivoDto> GetResumenEfectivoAsync(int turnoId, CancellationToken ct = default)
    {
        var turno = await Context.Turnos
            .FirstAsync(t => t.TurnoId == turnoId, ct);

        // We use EsEfectivo through joining with MetodosPago since the MetodoPago on Transaccion is a string
        var ventasEfectivo = await Context.Set<Transaccion>()
            .Where(t => t.TurnoId == turnoId && t.Anulacion == null)
            .Join(Context.Set<MetodoPago>(), t => t.MetodoPagoId, m => m.MetodoPagoId, (t, m) => new { t, m })
            .Where(x => x.m.EsEfectivo)
            .SumAsync(x => (decimal?)x.t.Total ?? 0, ct);

        var anulacionesEfectivo = await Context.Set<Transaccion>()
            .Where(t => t.TurnoId == turnoId && t.Anulacion != null)
            .Join(Context.Set<MetodoPago>(), t => t.MetodoPagoId, m => m.MetodoPagoId, (t, m) => new { t, m })
            .Where(x => x.m.EsEfectivo)
            .SumAsync(x => (decimal?)x.t.Total ?? 0, ct);

        var entradas = await Context.Set<MovimientoCaja>()
            .Where(m => m.TurnoId == turnoId && m.TipoMovimiento == TipoMovimiento.Entrada)
            .SumAsync(m => (decimal?)m.Monto ?? 0, ct);

        var salidas = await Context.Set<MovimientoCaja>()
            .Where(m => m.TurnoId == turnoId && m.TipoMovimiento == TipoMovimiento.Salida)
            .SumAsync(m => (decimal?)m.Monto ?? 0, ct);

        return new ResumenEfectivoDto(
            turno.MontoApertura,
            ventasEfectivo,
            anulacionesEfectivo,
            entradas,
            salidas);
    }
}
