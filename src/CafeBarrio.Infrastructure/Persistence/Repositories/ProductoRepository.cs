using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MUIS_CORE.Pagination;

namespace CafeBarrio.Infrastructure.Persistence.Repositories;

public class ProductoRepository : BaseRepository<Producto>, IProductoRepository
{
    public ProductoRepository(CafeBarrioDbContext context) : base(context) { }

    public override async Task<PagedResult<Producto>> GetPagedAsync(PaginationRequest request, CancellationToken ct = default)
    {
        if (request is not OffsetPaginationRequest offset)
            throw new NotSupportedException("Solo se soporta OffsetPaginationRequest.");

        var query = Context.Set<Producto>()
            .Include(p => p.Categoria)
            .AsNoTracking();

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((offset.PageNumber - 1) * offset.PageSize)
            .Take(offset.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Producto>(items, totalCount, offset.PageNumber, offset.PageSize);
    }

    public async Task<IReadOnlyList<Producto>> GetActivosAsync(CancellationToken ct = default)
        => await Context.Set<Producto>()
            .Include(p => p.Categoria)
            .Where(p => p.Activo)
            .AsNoTracking()
            .ToListAsync(ct);
}
