using MediatR;
using PostIQ.Core.Database;
using PostIQ.Core.Response;
using User.Application.Queries;
using User.Application.Response;
using User.Core.Entities;
using User.Core.Persistence;

namespace User.Application.Handlers;

public class GetUserDetailsByGuidHandler : IRequestHandler<GetUserDetailsByGuidQuery, SingleResponse<UserResponse>>
{
    private readonly IRepositoryAsync<UserDetail> _userDetails;

    public GetUserDetailsByGuidHandler(IUnitOfWork<UserDBContext> unitOfWork)
    {
        _userDetails = unitOfWork.GetRepositoryAsync<UserDetail>();
    }

    public async Task<SingleResponse<UserResponse>> Handle(GetUserDetailsByGuidQuery request, CancellationToken cancellationToken)
    {
        var userDetails = await _userDetails.SingleOrDefaultAsync(user => user.AuthId == request.UserGuid && user.IsActive);
        var response = userDetails is null
            ? null
            : new UserResponse(userDetails.UserId, userDetails.FirstName, userDetails.LastName, userDetails.ReferralCode);

        return new SingleResponse<UserResponse>(response);
    }
}