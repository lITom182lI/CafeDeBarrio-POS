using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Domain.Entities;

namespace CafeBarrio.Infrastructure.Persistence.Repositories;

public class AnulacionRepository : BaseRepository<Anulacion>, IAnulacionRepository
{
    public AnulacionRepository(CafeBarrioDbContext context) : base(context) { }
}
