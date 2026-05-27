using PostIQ.Core.Database.Entities;

namespace PostIQ.Core.Database.Extension
{
    public static class QueryFilterList<T>
    {
        public static async Task<IPaginate<T>> GetQueryFilterListAsync(IQueryable<T> query, FilterState state = null, CancellationToken cancellationToken = default)
        {
            if (state.Filter.Any()) query = query.Where(query.Filter(state.Filter));

            if (state.Sort.Any())
            {
                query = query.Sort(state.Sort);
                return await query.ToPaginateAsync(state.Skip, state.Take, 0, cancellationToken);
            }

            return await query.ToPaginateAsync(state.Skip, state.Take, 0, cancellationToken);
        }
    }
}
