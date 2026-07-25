using MediatR;
using PostIQ.Core.Database;
using User.Application.Queries;
using User.Core.Entities;
using User.Core.Persistence;

namespace User.Application.Handlers
{
    public class ValidateReferralHandler : IRequestHandler<ValidateReferralQuery, bool>
    {
        private readonly IUnitOfWork<UserDBContext> _uow;
        private readonly IRepositoryAsync<UserDetail> _userRepo;
        public ValidateReferralHandler(IUnitOfWork<UserDBContext> uow, RepositoryAsync<UserDetail> userRepo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow)); ;
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }
        public async Task<bool> Handle(ValidateReferralQuery request, CancellationToken cancellationToken)
        {
            var referrer = await _userRepo.SingleOrDefaultAsync(x => x.ReferralCode.ToLower() == request.code.ToLower() && x.IsActive);
            return referrer != null;
        }
    }
}
