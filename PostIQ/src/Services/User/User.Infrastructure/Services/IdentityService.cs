using MediatR;
using PostIQ.Core.Services;
using User.Application.Queries;

namespace User.Infrastructure.Services
{
    public sealed class IdentityService : IIdentityService
    {
        private readonly ITokenClaimsService _tokenClaimsService;
        private readonly IMediator _sender;

        public IdentityService(ITokenClaimsService tokenClaimsService, IMediator sender)
        {
            _tokenClaimsService = tokenClaimsService ?? throw new ArgumentNullException(nameof(tokenClaimsService));
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public async Task<IdentityDto> GetIdentityAsync(CancellationToken cancellationToken = default)
        {
            var userGuid = _tokenClaimsService.GetUserGuid();
            if (userGuid is null)
                return null;

            var result = await _sender.Send(new GetUserDetailsByGuidQuery(userGuid.Value), cancellationToken);
            var userDetails = result.Data;
            if (userDetails is null)
                return null;

            return new IdentityDto(userDetails.UserId, userGuid.Value, _tokenClaimsService.GetEmail());
        }
    }
}
