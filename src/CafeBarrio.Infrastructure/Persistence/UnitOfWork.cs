using CafeBarrio.Application.Common.Interfaces;

namespace CafeBarrio.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly CafeBarrioDbContext _context;

    public UnitOfWork(CafeBarrioDbContext context) => _context = context;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
