using Microsoft.EntityFrameworkCore;

namespace PollSystemApp.Application.Common.Pagination
{
    public class PagedList<T> : List<T>
    {
        public PaginationMetadata MetaData { get; }

        public PagedList(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            MetaData = new PaginationMetadata(totalCount, pageNumber, pageSize);
            AddRange(items);
        }

        public static async Task<PagedList<T>> CreateAsync(
            IQueryable<T> source,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var count = await source.CountAsync(cancellationToken);

            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}