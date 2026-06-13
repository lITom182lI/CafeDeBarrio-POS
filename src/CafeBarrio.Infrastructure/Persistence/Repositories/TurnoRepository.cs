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

        // Ventas efectivo — porción primaria (pago simple: MontoMetodoPrimario es null → usar Total)
        var ventasEfectivoPrimario = await Context.Set<Transaccion>()
            .Where(t => t.TurnoId == turnoId)
            .Join(Context.Set<MetodoPago>(),
                  t => t.MetodoPagoId,
                  m => m.MetodoPagoId,
                  (t, m) => new { t, m })
            .Where(x => x.m.EsEfectivo)
            .SumAsync(x => (decimal?)(x.t.MontoMetodoPrimario ?? x.t.Total), ct) ?? 0m;

        // Ventas efectivo — porción secundaria (pago dividido donde el segundo método es efectivo)
        var ventasEfectivoSecundario = await Context.Set<Transaccion>()
            .Where(t => t.TurnoId == turnoId
                     && t.MetodoPagoSecundarioId != null
                     && t.MontoMetodoPrimario != null)
            .Join(Context.Set<MetodoPago>(),
                  t => t.MetodoPagoSecundarioId!.Value,
                  m => m.MetodoPagoId,
                  (t, m) => new { t, m })
            .Where(x => x.m.EsEfectivo)
            .SumAsync(x => (decimal?)(x.t.Total - x.t.MontoMetodoPrimario!.Value), ct) ?? 0m;

        var ventasEfectivo = ventasEfectivoPrimario + ventasEfectivoSecundario;

        // Anulaciones con devolución en efectivo ocurridas durante este turno
        // Se filtra por FechaHora (no por TurnoId de la venta) para capturar correctamente
        // las devoluciones cross-turno: el dinero sale de la caja del turno donde se devolvió,
        // no del turno donde se realizó la venta original.
        var anulacionesEfectivo = await Context.Set<Anulacion>()
            .Where(a => a.Transaccion.SedeId == turno.SedeId
                     && a.FechaHora >= turno.FechaApertura
                     && (turno.FechaCierre == null || a.FechaHora < turno.FechaCierre)
                     && a.MetodoDevolucion == "Efectivo")
            .SumAsync(a => (decimal?)a.MontoDevuelto, ct) ?? 0m;

        var entradas = await Context.Set<MovimientoCaja>()
            .Where(m => m.TurnoId == turnoId && m.TipoMovimiento == TipoMovimiento.Entrada)
            .SumAsync(m => (decimal?)m.Monto, ct) ?? 0m;

        var salidas = await Context.Set<MovimientoCaja>()
            .Where(m => m.TurnoId == turnoId && m.TipoMovimiento == TipoMovimiento.Salida)
            .SumAsync(m => (decimal?)m.Monto, ct) ?? 0m;

        return new ResumenEfectivoDto(
            turno.MontoApertura,
            ventasEfectivo,
            anulacionesEfectivo,
            entradas,
            salidas);
    }
}
