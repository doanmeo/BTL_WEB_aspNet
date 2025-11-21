using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BlogWebsite.Models.ViewModels
{
    public interface IPagedResult
    {
        int PageIndex { get; }
        int TotalPages { get; }
        int PageSize { get; }
        int TotalCount { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
    }

    public class PagedResult<T> : IPagedResult
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int PageIndex { get; init; }
        public int TotalPages { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static PagedResult<T> Empty(int pageSize = 10) =>
            new PagedResult<T>
            {
                Items = Array.Empty<T>(),
                PageIndex = 1,
                TotalPages = 1,
                PageSize = pageSize,
                TotalCount = 0
            };

        public static async Task<PagedResult<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            if (pageSize <= 0) pageSize = 10;
            pageIndex = Math.Max(pageIndex, 1);

            var totalCount = await source.CountAsync(cancellationToken);
            var totalPages = Math.Max((int)Math.Ceiling(totalCount / (double)pageSize), 1);
            if (pageIndex > totalPages) pageIndex = totalPages;

            var items = await source.Skip((pageIndex - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync(cancellationToken);

            return new PagedResult<T>
            {
                Items = items,
                PageIndex = pageIndex,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}

