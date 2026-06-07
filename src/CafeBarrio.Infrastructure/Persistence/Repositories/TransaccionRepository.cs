using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafeBarrio.Infrastructure.Persistence.Repositories;

public class TransaccionRepository : BaseRepository<Transaccion>, ITransaccionRepository
{
    public TransaccionRepository(CafeBarrioDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Transaccion>> GetBySedeFechaAsync(
        int sedeId, DateTime desde, DateTime hasta, CancellationToken ct = default)
        => await Context.Set<Transaccion>()
            .Where(t => t.SedeId == sedeId && t.Fecha >= desde && t.Fecha <= hasta)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<Transaccion?> GetWithDetallesAndAnulacionAsync(int id, CancellationToken ct = default)
        => await Context.Set<Transaccion>()
            .Include(t => t.Detalles)
            .Include(t => t.Anulacion)
            .FirstOrDefaultAsync(t => t.TransaccionId == id, ct);
}
