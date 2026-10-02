using MediatR;
using PostIQ.Core.Database;
using PostIQ.Core.Response;
using User.Application.Queries;
using User.Core.Entities;
using User.Core.Persistence;

namespace User.Application.Handlers;

public class GetUserDetailsByIdHandler : IRequestHandler<GetUserDetailsByIdQuery, SingleResponse<UserDetail>>
{
    private readonly IRepositoryAsync<UserDetail> _userDetails;

    public GetUserDetailsByIdHandler(IUnitOfWork<UserDBContext> unitOfWork)
    {
        _userDetails = unitOfWork.GetRepositoryAsync<UserDetail>();
    }

    public async Task<SingleResponse<UserDetail>> Handle(GetUserDetailsByIdQuery request, CancellationToken cancellationToken)
    {
        var userDetails = await _userDetails.SingleOrDefaultAsync(user => user.UserId == request.UserId);
        return new SingleResponse<UserDetail>(userDetails);
    }
}