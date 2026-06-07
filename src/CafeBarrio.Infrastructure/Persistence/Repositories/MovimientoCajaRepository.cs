using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafeBarrio.Infrastructure.Persistence.Repositories;

public class MovimientoCajaRepository : BaseRepository<MovimientoCaja>, IMovimientoCajaRepository
{
    public MovimientoCajaRepository(CafeBarrioDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<MovimientoCaja>> GetByTurnoAsync(int turnoId, CancellationToken ct = default)
    {
        return await Context.Set<MovimientoCaja>()
            .AsNoTracking()
            .Where(m => m.TurnoId == turnoId)
            .OrderBy(m => m.FechaHora)
            .ToListAsync(ct);
    }
}
