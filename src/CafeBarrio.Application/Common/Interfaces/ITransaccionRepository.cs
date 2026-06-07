using CafeBarrio.Domain.Entities;
using MUIS_CORE.Interfaces;

namespace CafeBarrio.Application.Common.Interfaces;

public interface ITransaccionRepository : IRepository<Transaccion>
{
    Task<IReadOnlyList<Transaccion>> GetBySedeFechaAsync(int sedeId, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Transaccion?> GetWithDetallesAndAnulacionAsync(int id, CancellationToken ct = default);
}
