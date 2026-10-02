using MediatR;
using PostIQ.Core.Response;
using Published.Application.Response;

namespace Published.Application.Queries
{
    public record GetCommentsQuery(long PostId) : IRequest<ListResponse<CommentResponse>>;
}
