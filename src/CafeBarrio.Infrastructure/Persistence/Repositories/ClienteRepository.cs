using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafeBarrio.Infrastructure.Persistence.Repositories;

public class ClienteRepository : BaseRepository<Cliente>, IClienteRepository
{
    public ClienteRepository(CafeBarrioDbContext context) : base(context) { }

    public async Task<Cliente?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await Context.Set<Cliente>()
            .FirstOrDefaultAsync(c => c.Email == email, ct);
}
