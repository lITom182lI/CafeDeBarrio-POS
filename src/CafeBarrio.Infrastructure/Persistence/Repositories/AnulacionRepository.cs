using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafeBarrio.Infrastructure.Persistence.Repositories;

public class AnulacionRepository : BaseRepository<Anulacion>, IAnulacionRepository
{
    public AnulacionRepository(CafeBarrioDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Anulacion>> GetBySedeAsync(int sedeId, CancellationToken ct = default)
        => await Context.Set<Anulacion>()
            .Include(a => a.OperadorSolicitante)
            .Include(a => a.Autorizador)
            .Include(a => a.Transaccion)
            .Where(a => a.Transaccion.SedeId == sedeId)
            .OrderByDescending(a => a.FechaHora)
            .AsNoTracking()
            .ToListAsync(ct);
}
