using Microsoft.EntityFrameworkCore;

namespace Smlp.Application.Common.Models;

public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = count;
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
    }

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var page = pageNumber < 1 ? 1 : pageNumber;
        var take = pageSize < 1 ? 1 : pageSize;

        var count = await source.CountAsync(cancellationToken);
        var items = await source.Skip((page - 1) * take).Take(take).ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, count, page, take);
    }
}
