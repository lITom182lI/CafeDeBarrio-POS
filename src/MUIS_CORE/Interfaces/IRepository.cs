using System.Threading;
using System.Threading.Tasks;
using MUIS_CORE.Pagination;
using MUIS_CORE.Wrappers;

namespace MUIS_CORE.Interfaces;

// Contrato base de repositorio genérico para entidades con clave primaria de tipo TId.
// Implementar en la capa Infrastructure de cada proyecto — no en Domain.
public interface IRepository<T, TId>
    where T : class
    where TId : notnull
{
    Task<T?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task<PagedResult<T>> GetPagedAsync(PaginationRequest request, CancellationToken ct = default);
    Task<Result<TId>> AddAsync(T entity, CancellationToken ct = default);
    Task<Result> UpdateAsync(T entity, CancellationToken ct = default);
    Task<Result> DeleteAsync(TId id, CancellationToken ct = default);
}

// Alias conveniente para entidades con Id de tipo int (Tipo 1 y 2 — más común).
public interface IRepository<T> : IRepository<T, int> where T : class { }
