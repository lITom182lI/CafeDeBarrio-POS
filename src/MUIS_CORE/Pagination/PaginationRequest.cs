namespace MUIS_CORE.Pagination;

public abstract record PaginationRequest(int PageSize);

public record OffsetPaginationRequest(int PageNumber, int PageSize) : PaginationRequest(PageSize);

public record KeysetPaginationRequest(long LastSeenId, int PageSize) : PaginationRequest(PageSize);
