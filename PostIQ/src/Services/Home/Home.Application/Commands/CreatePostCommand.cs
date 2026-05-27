using MediatR;
using PostIQ.Core.Response;

namespace Home.Application.Commands
{
    public class CreatePostCommand : IRequest<SingleResponse<long>>
    {
        public long UserId { get; set; }
        public string Author { get; set; }
        public string Text { get; set; }
    }
}
