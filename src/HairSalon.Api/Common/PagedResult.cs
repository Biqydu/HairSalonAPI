using System.Text.Json.Serialization;

namespace HairSalon.Api.Common;

public readonly record struct PagedResult<T>
{
    [JsonConstructor]
    public PagedResult(IReadOnlyCollection<T> items, int pageNumber, int pageSize, int totalCount)
    {
        Items = items;
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize < 1 ? 10 : pageSize;
        TotalCount = totalCount < 0 ? 0 : totalCount;
        TotalPages = TotalCount == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public IReadOnlyCollection<T> Items { get; }

    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}