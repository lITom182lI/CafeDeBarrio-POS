using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafeBarrio.Infrastructure.Persistence.Repositories;

public class SedeRepository : BaseRepository<Sede>, ISedeRepository
{
    public SedeRepository(CafeBarrioDbContext context) : base(context) { }

    public async Task<int> GetDefaultSedeIdAsync(CancellationToken ct = default)
    {
        return await Context.Set<Sede>().Select(s => s.SedeId).FirstOrDefaultAsync(ct);
    }
}
