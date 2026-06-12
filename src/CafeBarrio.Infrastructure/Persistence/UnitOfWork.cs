using CafeBarrio.Application.Common.Interfaces;

namespace CafeBarrio.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable, IDisposable
{
    private readonly CafeBarrioDbContext _context;
    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _transaction;
    private bool _ownsTransaction;

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
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            throw new CafeBarrio.Application.Common.Exceptions.PersistenceException();
        }
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_context.Database.CurrentTransaction is not null)
        {
            // Transacción ambiente activa (p.ej. test harness) — reutilizar sin apropiarse
            _transaction = _context.Database.CurrentTransaction;
            _ownsTransaction = false;
            return;
        }
        _transaction = await _context.Database.BeginTransactionAsync(ct);
        _ownsTransaction = true;
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_transaction is not null && _ownsTransaction)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_transaction is not null && _ownsTransaction)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        if (_ownsTransaction) _transaction?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null && _ownsTransaction)
            await _transaction.DisposeAsync();
    }
}
