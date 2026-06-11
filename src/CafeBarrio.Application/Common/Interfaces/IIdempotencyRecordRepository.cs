using CafeBarrio.Domain.Entities;
using MUIS_CORE.Interfaces;

namespace CafeBarrio.Application.Common.Interfaces;

public interface IIdempotencyRecordRepository : IRepository<IdempotencyRecord>
{
    Task<IdempotencyRecord?> GetByKeyAsync(string idempotencyKey, CancellationToken ct = default);
}
