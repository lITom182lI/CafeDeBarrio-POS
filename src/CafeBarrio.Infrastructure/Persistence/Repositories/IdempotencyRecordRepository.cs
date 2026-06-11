using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafeBarrio.Infrastructure.Persistence.Repositories;

public class IdempotencyRecordRepository : BaseRepository<IdempotencyRecord>, IIdempotencyRecordRepository
{
    public IdempotencyRecordRepository(CafeBarrioDbContext context) : base(context) { }

    public async Task<IdempotencyRecord?> GetByKeyAsync(string idempotencyKey, CancellationToken ct = default)
        => await Context.Set<IdempotencyRecord>()
               .FirstOrDefaultAsync(r => r.IdempotencyKey == idempotencyKey, ct);
}
