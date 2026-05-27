using Home.Application.Response;
using MediatR;
using PostIQ.Core.Response;

namespace Home.Application.Queries
{
    public record GetCommentsQuery(long PostId) : IRequest<ListResponse<CommentResponse>>;
}
