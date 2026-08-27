using MediatR;
using Microsoft.Extensions.Logging;
using PostIQ.Core.Database;
using PostIQ.Core.HttpClientService.Services;
using PostIQ.Core.Response;
using User.Application.Queries;
using User.Core.Entities;
using User.Core.Persistence;

namespace User.Application.Handlers
{
    public class ValidateReferralHandler : IRequestHandler<ValidateReferralQuery, SingleResponse<bool>>
    {
        private readonly IUnitOfWork<UserDBContext> _uow;
        private readonly IRepositoryAsync<UserDetail> _userRepo;

        public ValidateReferralHandler(
            IUnitOfWork<UserDBContext> uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _userRepo = uow.GetRepositoryAsync<UserDetail>();
        }
        public async Task<SingleResponse<bool>> Handle(ValidateReferralQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var referrer = await _userRepo.SingleOrDefaultAsync(x => x.ReferralCode.ToLower() == request.code.ToLower() && x.IsActive);
                return new SingleResponse<bool>(referrer != null);
            }
            catch (Exception ex)
            {

                throw;
            }            
        }
    }
}
