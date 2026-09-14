using AMYL.Api.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AMYL.Api.Infrastructure.Persistence;

public static class QueryableExtensions
{
    public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<T>
        {
            Value = items,
            PageSize = pageSize,
            PageNumber = pageNumber,
            TotalCount = totalCount
        };
    }

    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate)
        => condition ? query.Where(predicate) : query;
}
