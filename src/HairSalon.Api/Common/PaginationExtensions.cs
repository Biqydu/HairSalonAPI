using Microsoft.EntityFrameworkCore;

namespace HairSalon.Api.Common;

public static class PaginationExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int pageNumber,
        int pageSize, CancellationToken ct = default)
    {
        var totalCount = await query.CountAsync(ct);

        if (totalCount == 0)
            return new PagedResult<T>([], pageNumber, pageSize, 0);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>(items, pageNumber, pageSize, totalCount);
    }
}