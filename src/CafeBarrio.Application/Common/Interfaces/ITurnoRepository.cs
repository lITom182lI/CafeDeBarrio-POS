using CafeBarrio.Application.Features.Reportes.Dtos;
using CafeBarrio.Domain.Entities;
using MUIS_CORE.Interfaces;

namespace CafeBarrio.Application.Common.Interfaces;

public interface ITurnoRepository : IRepository<Turno>
{
    Task<Turno?> GetActivoBySedeAsync(int sedeId, CancellationToken ct = default);
    Task<IReadOnlyList<TurnoHistorialDto>> GetHistorialBySedeAsync(int sedeId, int top, CancellationToken ct = default);
    Task<decimal> GetTotalEfectivoByTurnoAsync(int turnoId, CancellationToken ct = default);
}
