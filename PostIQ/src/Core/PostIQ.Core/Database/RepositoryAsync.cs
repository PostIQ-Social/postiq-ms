using PostIQ.Core.Database.Entities;
using PostIQ.Core.Database.Extension;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace PostIQ.Core.Database
{
    public class RepositoryAsync<T> : IRepositoryAsync<T> where T : class
    {
        private readonly DbSet<T> _dbSet;

        public RepositoryAsync(DbContext dbContext)
        {
            _dbSet = dbContext.Set<T>();
        }

        #region SingleOrDefault



        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        {
            return await SingleOrDefaultAsync(predicate, orderBy, default);
        }

        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, Func<IQueryable<T>, IIncludableQueryable<T, object>> include)
        {
            return await SingleOrDefaultAsync(predicate, orderBy, include, default);
        }

        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, Func<IQueryable<T>, IIncludableQueryable<T, object>> include, bool enableTracking)
        {
            return await SingleOrDefaultAsync(predicate, orderBy, include, enableTracking, default);
        }

        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            bool enableTracking = true,
            bool ignoreQueryFilters = false)
        {
            IQueryable<T> query = _dbSet;

            if (!enableTracking) query = query.AsNoTracking();

            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            if (ignoreQueryFilters) query = query.IgnoreQueryFilters();

            if (orderBy != null) return await orderBy(query).FirstOrDefaultAsync();

            return await query.FirstOrDefaultAsync();
        }
        public async Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            bool disableTracking = true,
            bool ignoreQueryFilters = false)
        {
            IQueryable<T> query = _dbSet;

            if (disableTracking) query = query.AsNoTracking();

            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            if (ignoreQueryFilters) query = query.IgnoreQueryFilters();

            return orderBy != null ? await orderBy(query).Select(selector).SingleOrDefaultAsync() : await query.Select(selector).SingleOrDefaultAsync();
        }

        public async Task<IPaginate<T>> GetListAsync(Expression<Func<T, bool>> predicate)
        {
            return await GetListAsync(predicate, default);
        }

        public async Task<IPaginate<T>> GetListAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        {
            return await GetListAsync(predicate, orderBy, default);
        }

        public async Task<IPaginate<T>> GetListAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, Func<IQueryable<T>, IIncludableQueryable<T, object>> include)
        {
            return await GetListAsync(predicate, orderBy, include, default);
        }

        public async Task<IPaginate<T>> GetListAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, Func<IQueryable<T>, IIncludableQueryable<T, object>> include, int index, int size)
        {
            return await GetListAsync(predicate, orderBy, include, index, size, default);
        }

        #endregion

        #region GetListAsync

        public async Task<IPaginate<T>> GetListAsync(Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            int index = 0,
            int size = 50,
            bool enableTracking = true,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;
            if (!enableTracking) query = query.AsNoTracking();

            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            if (orderBy != null)
                return await orderBy(query).ToPaginateAsync(index, size, 0, cancellationToken);
            return await query.ToPaginateAsync(index, size, 0, cancellationToken);
        }

        public async Task<IPaginate<T>> GetFilterListAsync(Expression<Func<T, bool>> predicate = null,
            FilterState state = null)
        {
            return await GetFilterListAsync(predicate, state, default);
        }

        public async Task<IPaginate<T>> GetFilterListAsync(Expression<Func<T, bool>> predicate = null, 
            FilterState state = null, 
            bool enableTracking = true, CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;
            if (!enableTracking) query = query.AsNoTracking();

            if (predicate != null) query = query.Where(predicate);

            if (state.Filter.Any()) query = query.Where(query.Filter(state.Filter));

            if (state.Sort.Any())
            {
                query = query.Sort(state.Sort);
                return await query.ToPaginateAsync(state.Skip, state.Take, 0, cancellationToken);
            }

            return await query.ToPaginateAsync(state.Skip, state.Take, 0, cancellationToken);
        }

        public Task<IPaginate<TResult>> GetListAsync<TResult>(Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            int index = 0,
            int size = 50,
            bool enableTracking = true,
            CancellationToken cancellationToken = default,
            bool ignoreQueryFilters = false)
            where TResult : class
        {
            IQueryable<T> query = _dbSet;

            if (!enableTracking) query = query.AsNoTracking();

            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            if (ignoreQueryFilters) query = query.IgnoreQueryFilters();

            return orderBy != null ? orderBy(query).Select(selector).ToPaginateAsync(index, size, 0, cancellationToken) : query.Select(selector).ToPaginateAsync(index, size, 0, cancellationToken);
        }

        #endregion

        #region Insert Functions

        public virtual ValueTask<EntityEntry<T>> InsertAsync(T entity, CancellationToken cancellationToken = default)
        {
            return _dbSet.AddAsync(entity, cancellationToken);
        }

        public virtual Task InsertAsync(params T[] entities)
        {
            return _dbSet.AddRangeAsync(entities);
        }

        public virtual Task InsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            return _dbSet.AddRangeAsync(entities, cancellationToken);
        }      

        #endregion
    }
}
