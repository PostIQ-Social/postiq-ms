using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace PostIQ.Core.Database
{
    public sealed class Repository<T> : BaseRepository<T>, IRepository<T> where T : class
    {
        public Repository(DbContext context) : base(context)
        {
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void Delete(params T[] entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public void Delete(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        #region Insert Functions

        public T Insert(T entity)
        {
            return _dbSet.Add(entity).Entity;
        }

        public void Insert(params T[] entities)
        {
            _dbSet.AddRange(entities);
        }

        public void Insert(IEnumerable<T> entities)
        {
            _dbSet.AddRange(entities);
        }

        public T InsertNotExists(Expression<Func<T, bool>> predicate, T entity)
        {
            if (_dbSet.Any(predicate)) return _dbSet.SingleOrDefault(predicate.Compile());
            _dbSet.Add(entity);
            return entity;
        }

        #endregion


        #region Update Functions

        public void Update(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            _dbSet.Update(entity);

        }

        public void Update(params T[] entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public void Update(IEnumerable<T> entities)
        {

            _dbSet.UpdateRange(entities);
        }

        #endregion
    }
}
