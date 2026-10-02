using MediatR;
using PostIQ.Core.Response;
using User.Core.Entities;

namespace User.Application.Queries;

public record GetUserDetailsByIdQuery(long UserId) : IRequest<SingleResponse<UserDetail>>;