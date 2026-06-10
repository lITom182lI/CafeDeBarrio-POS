using CafeBarrio.Application.Common.Interfaces;

namespace CafeBarrio.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly CafeBarrioDbContext _context;

    public UnitOfWork(CafeBarrioDbContext context) => _context = context;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await _context.SaveChangesAsync(ct);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            throw new CafeBarrio.Application.Common.Exceptions.ConcurrencyException();
        }
    }
}
