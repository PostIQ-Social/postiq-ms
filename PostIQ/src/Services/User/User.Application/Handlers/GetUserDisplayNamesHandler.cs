using MediatR;
using PostIQ.Core.Database;
using User.Application.Queries;
using User.Core.Entities;
using User.Core.Persistence;

namespace User.Application.Handlers;

public sealed class GetUserDisplayNamesHandler : IRequestHandler<GetUserDisplayNamesQuery, Dictionary<long, string>>
{
    private readonly IRepositoryAsync<UserDetail> _userDetails;

    public GetUserDisplayNamesHandler(IUnitOfWork<UserDBContext> unitOfWork)
    {
        _userDetails = unitOfWork.GetRepositoryAsync<UserDetail>();
    }

    public async Task<Dictionary<long, string>> Handle(GetUserDisplayNamesQuery request, CancellationToken cancellationToken)
    {
        var result = await _userDetails.GetListAsync(
            predicate: user => request.UserIds.Contains(user.UserId),
            enableTracking: false,
            cancellationToken: cancellationToken);

        return result.Data.ToDictionary(
            user => user.UserId,
            user => $"{user.FirstName} {user.LastName}".Trim());
    }
}