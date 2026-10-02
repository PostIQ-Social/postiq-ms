using MediatR;
using PostIQ.Core.Response;
using User.Application.Response;

namespace User.Application.Queries;

public record GetUserDetailsByGuidQuery(Guid UserGuid) : IRequest<SingleResponse<UserResponse>>;