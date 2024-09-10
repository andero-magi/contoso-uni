using Microsoft.EntityFrameworkCore;

namespace ContosoUni
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pgIndex, int pgSize) 
        {
            PageIndex = pgIndex;
            TotalPages = (int) Math.Ceiling(count / (double) pgSize);
            AddRange(items);
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pgIndex, int pgSize)
        {
            int count = await source.CountAsync();
            List<T> items = await source.Skip((pgIndex - 1) * pgSize).Take(pgSize).ToListAsync();
            return new PaginatedList<T>(items, count, pgIndex, pgSize);
        }

        public bool HasPerviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}
