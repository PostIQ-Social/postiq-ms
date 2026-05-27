using MediatR;
using PostIQ.Core.Response;

namespace Home.Application.Commands
{
    public class LikeCommentCommand : IRequest<SingleResponse<bool>>
    {
        public long CommentId { get; set; }
        public long UserId { get; set; }
    }
}
