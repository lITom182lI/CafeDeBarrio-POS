using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafeBarrio.Infrastructure.Persistence.Repositories;

public class OperadorRepository : BaseRepository<Operador>, IOperadorRepository
{
    public OperadorRepository(CafeBarrioDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Operador>> GetAllAsync(CancellationToken ct = default)
        => await Context.Set<Operador>()
            .OrderBy(o => o.Nombre)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Operador>> GetAllActivosAsync(CancellationToken ct = default)
        => await Context.Set<Operador>()
            .Where(o => o.Activo)
            .OrderBy(o => o.Nombre)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<Operador?> GetActivoByIdAsync(int id, CancellationToken ct = default)
        => await Context.Set<Operador>()
            .FirstOrDefaultAsync(o => o.OperadorId == id && o.Activo, ct);
}
