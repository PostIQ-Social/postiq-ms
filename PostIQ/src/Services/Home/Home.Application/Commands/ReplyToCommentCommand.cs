using MediatR;
using PostIQ.Core.Response;

namespace Home.Application.Commands
{
    public class ReplyToCommentCommand : IRequest<SingleResponse<bool>>
    {
        public long PostId { get; set; }
        public long ParentCommentId { get; set; }
        public long UserId { get; set; }
        public string Content { get; set; }
    }
}
