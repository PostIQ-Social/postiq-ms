using PostIQ.Core.Database;
using User.Core.Entities;
using User.Core.IRepository;
using User.Core.Persistence;

namespace User.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        protected internal IUnitOfWork<UserDBContext> _uow;
        public readonly IRepositoryAsync<Users> repoAsync;
        public readonly IRepository<Users> repo;
        public readonly IRepositoryReadOnly<Users> readOnly;
        public readonly IRepositoryReadOnlyAsync<Users> readOnlyAsync;
        public UserRepository(IUnitOfWork<UserDBContext> uow)
        {
            _uow = uow;
            repoAsync = _uow.GetRepositoryAsync<Users>();
            repo = _uow.GetRepository<Users>();
            readOnly = _uow.GetReadOnlyRepository<Users>();
            readOnlyAsync = _uow.GetReadOnlyRepositoryAsync<Users>();
        }
    }
}
