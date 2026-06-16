using MediatR;
using PostIQ.Core.Response;

namespace Home.Application.Commands
{
    public class CommentPostCommand : IRequest<SingleResponse<bool>>
    {
        public long PostId { get; set; }
        public long UserId { get; set; }
        public string Content { get; set; }
    }
}
