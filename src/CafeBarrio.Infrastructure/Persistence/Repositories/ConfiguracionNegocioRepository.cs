using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafeBarrio.Infrastructure.Persistence.Repositories;

public class ConfiguracionNegocioRepository : BaseRepository<ConfiguracionNegocio>, IConfiguracionNegocioRepository
{
    public ConfiguracionNegocioRepository(CafeBarrioDbContext context) : base(context) { }

    public async Task<ConfiguracionNegocio?> GetActivaBySedeAsync(int sedeId, CancellationToken ct = default)
        => await Context.Set<ConfiguracionNegocio>()
               .Where(c => c.SedeId == sedeId && c.Activo)
               .OrderByDescending(c => c.FechaVigencia)
               .FirstOrDefaultAsync(ct);
}
