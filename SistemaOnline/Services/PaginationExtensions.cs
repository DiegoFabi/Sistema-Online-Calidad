using Microsoft.EntityFrameworkCore;
using SistemaOnline.ViewModels;

namespace SistemaOnline.Services
{
    public static class PaginationExtensions
    {
        public const int DefaultPageSize = 10;
        public static readonly int[] TamanosPaginaPermitidos = { 10, 20, 30, 50 };

        public static async Task<PagedResult<T>> ToPagedListAsync<T>(this IQueryable<T> source, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (!TamanosPaginaPermitidos.Contains(pageSize)) pageSize = DefaultPageSize;

            int totalCount = await source.CountAsync();
            int totalPages = pageSize <= 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}