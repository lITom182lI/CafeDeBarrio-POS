using System.Collections.Generic;

namespace MUIS_CORE.Pagination;

// Para Offset Pagination: TotalCount está disponible (permite calcular páginas totales).
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize) where T : class
{
    public bool HasNextPage => PageNumber * PageSize < TotalCount;
    public bool HasPreviousPage => PageNumber > 1;
    public int TotalPages => TotalCount == 0 ? 0 : (TotalCount + PageSize - 1) / PageSize;
}

// Para Keyset Pagination: TotalCount NO está disponible (evitar query costosa de COUNT).
// HasMore indica si existe al menos una página siguiente.
public record KeysetPagedResult<T>(
    IReadOnlyList<T> Items,
    bool HasMore) where T : class;
