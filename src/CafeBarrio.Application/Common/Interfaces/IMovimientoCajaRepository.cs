using CafeBarrio.Domain.Entities;
using MUIS_CORE.Interfaces;

namespace CafeBarrio.Application.Common.Interfaces;

public interface IMovimientoCajaRepository : IRepository<MovimientoCaja>
{
    Task<IReadOnlyList<MovimientoCaja>> GetByTurnoAsync(int turnoId, CancellationToken ct = default);
}
