using CafeBarrio.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MUIS_CORE.Interfaces;
using MUIS_CORE.Pagination;
using MUIS_CORE.Wrappers;

namespace CafeBarrio.Infrastructure.Persistence.Repositories;

public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly CafeBarrioDbContext Context;

    protected BaseRepository(CafeBarrioDbContext context) => Context = context;

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await Context.Set<T>().FindAsync([id], ct);

    public virtual async Task<PagedResult<T>> GetPagedAsync(PaginationRequest request, CancellationToken ct = default)
    {
        if (request is not OffsetPaginationRequest offset)
            throw new NotSupportedException("Solo se soporta OffsetPaginationRequest.");

        var query = Context.Set<T>().AsNoTracking();
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((offset.PageNumber - 1) * offset.PageSize)
            .Take(offset.PageSize)
            .ToListAsync(ct);

        return new PagedResult<T>(items, totalCount, offset.PageNumber, offset.PageSize);
    }

    public Task<Result<int>> AddAsync(T entity, CancellationToken ct = default)
    {
        try
        {
            Context.Set<T>().Add(entity);
            return Task.FromResult(Result<int>.Success(0));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<int>.Failure(new Error("Db.AddError", ex.Message)));
        }
    }

    public Task<Result> UpdateAsync(T entity, CancellationToken ct = default)
    {
        try
        {
            Context.Set<T>().Update(entity);
            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure(new Error("Db.UpdateError", ex.Message)));
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var entity = await GetByIdAsync(id, ct);
            if (entity is null)
                return Result.Failure(new Error("Db.NotFound", $"Entidad con id {id} no encontrada."));
            Context.Set<T>().Remove(entity);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Db.DeleteError", ex.Message));
        }
    }
}
