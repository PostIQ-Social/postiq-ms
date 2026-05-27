using MediatR;
using PostIQ.Core.Response;
using User.Application.Response;

namespace User.Application.Queries
{
    public record GetUserPostsQuery(long UserId) : IRequest<List<PostResponse>>;
}
