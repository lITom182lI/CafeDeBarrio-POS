using CafeBarrio.Domain.Entities;
using MUIS_CORE.Interfaces;

namespace CafeBarrio.Application.Common.Interfaces;

public interface IProductoRepository : IRepository<Producto>
{
    Task<IReadOnlyList<Producto>> GetActivosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Producto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
}
