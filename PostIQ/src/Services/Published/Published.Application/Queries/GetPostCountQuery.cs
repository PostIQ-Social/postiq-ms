using MediatR;
using PostIQ.Core.Response;
using Published.Core.Entities;

namespace Published.Application.Queries
{
    public record GetPostCountQuery(long[] PostId) : IRequest<ListResponse<PostsCount>>;
}
