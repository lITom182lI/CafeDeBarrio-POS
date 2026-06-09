using CafeBarrio.Domain.Entities;
using MUIS_CORE.Interfaces;

namespace CafeBarrio.Application.Common.Interfaces;

public interface ISedeRepository : IRepository<Sede>
{
    Task<int> GetDefaultSedeIdAsync(CancellationToken ct = default);
}
