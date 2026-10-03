using MediatR;
using PostIQ.Core.Response;
using Published.Core.Entities;

namespace Published.Application.Queries
{
    public record GetJobForTriggerQuery(long UserId) : IRequest<ListResponse<Job>>;
}
