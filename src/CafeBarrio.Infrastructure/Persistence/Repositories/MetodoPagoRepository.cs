using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using CafeBarrio.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CafeBarrio.Infrastructure.Persistence.Repositories;

public class MetodoPagoRepository : BaseRepository<MetodoPago>, IMetodoPagoRepository
{
    public MetodoPagoRepository(CafeBarrioDbContext context) : base(context) { }

    public async Task<IReadOnlyList<MetodoPago>> GetAllAsync(CancellationToken ct = default)
        => await Context.Set<MetodoPago>()
            .AsNoTracking()
            .ToListAsync(ct);
}
