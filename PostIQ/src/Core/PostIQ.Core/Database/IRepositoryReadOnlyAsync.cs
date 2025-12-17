using PostIQ.Core.Database.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace PostIQ.Core.Database
{
    public interface IRepositoryReadOnlyAsync<T> where T : class
    {
        Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null
        );

        Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            bool disableTracking = true,
            bool ignoreQueryFilters = false);

        Task<IPaginate<T>> GetListAsync(Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            int index = 0,
            int size = 50);


        Task<IPaginate<T>> GetFilterListAsync(Expression<Func<T, bool>> predicate = null,
         FilterState state = null);


        Task<IPaginate<TResult>> GetListAsync<TResult>(Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            int index = 0,
            int size = 50,
            CancellationToken cancellationToken = default,
            bool ignoreQueryFilters = false)
            where TResult : class;
    }
}
