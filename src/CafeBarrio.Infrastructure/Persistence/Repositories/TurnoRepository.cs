using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Application.Features.Reportes.Dtos;
using CafeBarrio.Domain.Entities;
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

    public async Task<decimal> GetTotalEfectivoByTurnoAsync(int turnoId, CancellationToken ct = default)
        => await Context.Set<Transaccion>()
               .Where(t => t.TurnoId == turnoId)
               .Join(Context.Set<MetodoPago>(),
                     t => t.MetodoPagoId,
                     m => m.MetodoPagoId,
                     (t, m) => new { t.Total, m.EsEfectivo })
               .Where(x => x.EsEfectivo)
               .SumAsync(x => (decimal?)x.Total, ct) ?? 0;
}
