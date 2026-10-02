using MediatR;
using PostIQ.Core.Response;
using Published.Application.Response;

namespace Published.Application.Queries;

public record GetJobsByUserIdQuery(long UserId) : IRequest<ListResponse<UserJobResponse>>;