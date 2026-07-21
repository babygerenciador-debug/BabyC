namespace FleetOS.Shared.Pagination;

/// <summary>Paged result container for list queries.</summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PagedResult<T> Create(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize) =>
        new() { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };

    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 20) =>
        new() { Items = [], TotalCount = 0, PageNumber = pageNumber, PageSize = pageSize };
}

/// <summary>Standard pagination filter for queries.</summary>
public sealed record PageFilter(
    int PageNumber = 1,
    int PageSize = 20,
    string? Search = null,
    string? SortBy = null,
    bool SortDescending = false)
{
    public int Skip => (PageNumber - 1) * PageSize;
    public int Take => PageSize;

    public PageFilter Validated() => this with
    {
        PageNumber = PageNumber < 1 ? 1 : PageNumber,
        PageSize = PageSize < 1 ? 20 : PageSize > 100 ? 100 : PageSize
    };
}
