using CafeBarrio.Domain.Entities;
using MUIS_CORE.Interfaces;

namespace CafeBarrio.Application.Common.Interfaces;

public interface IOperadorRepository : IRepository<Operador>
{
    Task<IReadOnlyList<Operador>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Operador>> GetAllActivosAsync(CancellationToken ct = default);
    Task<Operador?> GetActivoByIdAsync(int id, CancellationToken ct = default);
}
