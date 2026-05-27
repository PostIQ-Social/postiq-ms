using PostIQ.Core.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PostIQ.Core.Database.Extension
{
    /// <summary>
    /// Provides extension methods for asynchronously paginating the results of an IQueryable sequence.
    /// </summary>
    /// <remarks>These extension methods enable efficient retrieval of paged data from queryable sources, such
    /// as Entity Framework queries. The methods are designed to work with asynchronous database providers and return
    /// paginated results that include both the data and pagination metadata.</remarks>
    /// <params>size = -1 : Its take records with out Skip() and Take()</params>
    /// <params>index = -1 : Its take records with only Take(), omit Skip() - (select top size * from Table) - for performance reason</params>
    public static class IQueryablePaginateExtensions
    {
        public static async Task<IPaginate<T>> ToPaginateAsync<T>(this IQueryable<T> source, int index = 0, int size = 50,
            int from = 0, CancellationToken cancellationToken = default)
        {
            if (from > index)
                from = 0;

            var count = await source.CountAsync(cancellationToken).ConfigureAwait(false);
            var items = new List<T>();
            if (size == -1)
                items = await source.ToListAsync(cancellationToken).ConfigureAwait(false);
            else
            {
                //items = await source.Skip((index - from) * size)
                //   .Take(size).ToListAsync(cancellationToken).ConfigureAwait(false);
                if (index >= 0)
                    items = await source.Skip(index)
                       .Take(size).ToListAsync(cancellationToken).ConfigureAwait(false);
                else
                    items = await source
                   .Take(size).ToListAsync(cancellationToken).ConfigureAwait(false);
            }               

            var list = new Paginate<T>
            {
                Index = index,
                Size = size,
                From = from,
                Count = count,
                Data = items,
                Pages = (int)Math.Ceiling(count / (double)size)
            };

            return list;
        }
    }
}
