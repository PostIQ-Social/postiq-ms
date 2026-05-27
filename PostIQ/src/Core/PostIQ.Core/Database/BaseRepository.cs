using PostIQ.Core.Database.Entities;
using PostIQ.Core.Database.Extension;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;


namespace PostIQ.Core.Database
{
    public abstract class BaseRepository<T> : IReadRepository<T> where T : class
    {
        protected readonly DbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(DbContext context)
        {
            _dbContext = context ?? throw new ArgumentException(nameof(context));
            _dbSet = _dbContext.Set<T>();
        }

        #region SingleOrDefault
        public T SingleOrDefault(Expression<Func<T, bool>> predicate)
        {
            return SingleOrDefault(predicate, default);
        }

        public T SingleOrDefault(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        {
            return SingleOrDefault(predicate, orderBy, default);
        }

        public T SingleOrDefault(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, Func<IQueryable<T>, IIncludableQueryable<T, object>> include)
        {
            return SingleOrDefault(predicate, orderBy, include, default);
        }

        public T SingleOrDefault(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include, bool enableTracking)
        {
            return SingleOrDefault(predicate, orderBy, include, enableTracking, default);
        }

        public T SingleOrDefault(Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include, bool enableTracking,
            bool ignoreQueryFilters)
        {
            IQueryable<T> query = _dbSet;

            if (!enableTracking) query = query.AsNoTracking();

            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            if (ignoreQueryFilters) query = query.IgnoreQueryFilters();

            return orderBy != null ? orderBy(query).FirstOrDefault() : query.FirstOrDefault();
        }

        public IPaginate<T> GetList(Expression<Func<T, bool>> predicate)
        {
            return GetList(predicate, default);
        }

        public IPaginate<T> GetList(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        {
            return GetList(predicate, orderBy, default);
        }

        public IPaginate<T> GetList(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, Func<IQueryable<T>, IIncludableQueryable<T, object>> include)
        {
            return GetList(predicate, orderBy, include, default);
        }

        public IPaginate<T> GetList(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, Func<IQueryable<T>, IIncludableQueryable<T, object>> include, int index, int size)
        {
            return GetList(predicate, orderBy, include, index, size, default);
        }

        #endregion

        public IPaginate<T> GetList(Expression<Func<T, bool>> predicate = null,
            int size = 50, bool enableTracking = true)
        {
            return GetList(predicate, 0, size, enableTracking);
        }

        public IPaginate<T> GetList(Expression<Func<T, bool>> predicate = null, int index = 0,
            int size = 50, bool enableTracking = true)
        {
            return GetList(predicate, null, index, size, enableTracking);
        }

        public IPaginate<T> GetList(Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, int index = 0,
            int size = 50, bool enableTracking = true)
        {
            return GetList(predicate, null, include, index, size, enableTracking);
        }

        public IPaginate<T> GetList(Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, int index = 0,
            int size = 50, bool enableTracking = true)
        {
            IQueryable<T> query = _dbSet;
            if (!enableTracking) query = query.AsNoTracking();

            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            return orderBy != null ? orderBy(query).ToPaginate(index, size) : query.ToPaginate(index, size);
        }


        public TResult SingleOrDefault<TResult>(Expression<Func<T, TResult>> selector,
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

            return orderBy != null ? orderBy(query).Select(selector).FirstOrDefault() : query.Select(selector).FirstOrDefault();
        }


        public IPaginate<TResult> GetList<TResult>(Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            int index = 0, int size = 50, bool enableTracking = true) where TResult : class
        {
            IQueryable<T> query = _dbSet;
            if (!enableTracking) query = query.AsNoTracking();

            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            return orderBy != null
                ? orderBy(query).Select(selector).ToPaginate(index, size)
                : query.Select(selector).ToPaginate(index, size);
        }

        public IPaginate<T> GetFilterList(Expression<Func<T, bool>> predicate = null,
          FilterState state = null,
          bool enableTracking = true)
        {
            IQueryable<T> query = _dbSet;
            if (!enableTracking) query = query.AsNoTracking();

            if (predicate != null) query = query.Where(predicate);

            if (state.Filter.Any()) query = query.Where(query.Filter(state.Filter));

            if (state.Sort.Any())
            {
                query = query.Sort(state.Sort);
                return query.ToPaginate(state.Skip, state.Take);
            }

            return query.ToPaginate(state.Skip, state.Take);
        }
    }
}
