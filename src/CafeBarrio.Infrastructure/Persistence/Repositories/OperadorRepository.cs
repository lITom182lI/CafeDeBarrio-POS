using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;

namespace CafeBarrio.Infrastructure.Persistence.Repositories;

public class OperadorRepository : BaseRepository<Operador>, IOperadorRepository
{
    public OperadorRepository(CafeBarrioDbContext context) : base(context) { }
}
