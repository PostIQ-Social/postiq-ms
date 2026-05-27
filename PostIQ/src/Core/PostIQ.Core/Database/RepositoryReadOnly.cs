using PostIQ.Core.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace PostIQ.Core.Database
{
    public class RepositoryReadOnly<T> : BaseRepository<T>, IRepositoryReadOnly<T> where T : class
    {
        public RepositoryReadOnly(DbContext context) : base(context)
        {
        }

        public IPaginate<TResult> GetList<TResult>(Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            int index = 0, int size = 50) where TResult : class
        {
            return base.GetList(selector, predicate, orderBy, include, index, size, false);
        }

        public IPaginate<T> GetFilterList(Expression<Func<T, bool>> predicate = null,
         FilterState state = null)
        {
            return base.GetFilterList(predicate, state, false);
        }
    }
}
