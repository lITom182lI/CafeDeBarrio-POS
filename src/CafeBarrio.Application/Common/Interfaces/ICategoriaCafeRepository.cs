using CafeBarrio.Domain.Entities;
using MUIS_CORE.Interfaces;

namespace CafeBarrio.Application.Common.Interfaces;

public interface ICategoriaCafeRepository : IRepository<CategoriaCafe>
{
    Task<IReadOnlyList<CategoriaCafe>> GetAllActivasAsync(CancellationToken ct = default);
}
