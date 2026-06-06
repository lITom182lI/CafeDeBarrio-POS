using CafeBarrio.Domain.Entities;
using MUIS_CORE.Interfaces;

namespace CafeBarrio.Application.Common.Interfaces;

public interface IConfiguracionNegocioRepository : IRepository<ConfiguracionNegocio>
{
    Task<ConfiguracionNegocio?> GetActivaBySedeAsync(int sedeId, CancellationToken ct = default);
}
