using PostIQ.Core.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace PostIQ.Core.Database.Extension
{
    public static class IQueryablePaginateExtensions
    {
        public static async Task<IPaginate<T>> ToPaginateAsync<T>(this IQueryable<T> source, int index = 0, int size = 50,
            int from = 0, CancellationToken cancellationToken = default)
        {
            if (from > index) throw new ArgumentException($"From: {from} > Index: {index}, must From <= Index");

            var count = await source.CountAsync(cancellationToken).ConfigureAwait(false);
            var items = new List<T>();
            if (size == -1)
                items = await source.ToListAsync(cancellationToken).ConfigureAwait(false);
            else
            {
                //items = await source.Skip((index - from) * size)
                //   .Take(size).ToListAsync(cancellationToken).ConfigureAwait(false);

                items = await source.Skip(index)
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
