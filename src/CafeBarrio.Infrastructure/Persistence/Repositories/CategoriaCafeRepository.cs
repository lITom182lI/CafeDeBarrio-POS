using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using CafeBarrio.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CafeBarrio.Infrastructure.Persistence.Repositories;

public class CategoriaCafeRepository : BaseRepository<CategoriaCafe>, ICategoriaCafeRepository
{
    public CategoriaCafeRepository(CafeBarrioDbContext context) : base(context) { }

    public async Task<IReadOnlyList<CategoriaCafe>> GetAllActivasAsync(CancellationToken ct = default)
        => await Context.CategoriasCafe
            .Where(c => c.Activa)
            .OrderBy(c => c.Nombre)
            .AsNoTracking()
            .ToListAsync(ct);
}
