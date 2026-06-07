using CafeBarrio.Domain.Entities;
using MUIS_CORE.Interfaces;

namespace CafeBarrio.Application.Common.Interfaces;

public interface IMetodoPagoRepository : IRepository<MetodoPago>
{
    Task<IReadOnlyList<MetodoPago>> GetAllAsync(CancellationToken ct = default);
}
