using Home.Application.Commands;
using Home.Core.Entities;
using Home.Core.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PostIQ.Core.Response;
using System.Threading;
using System.Threading.Tasks;

namespace Home.Application.Handlers
{
    public class ReplyToCommentCommandHandler : IRequestHandler<ReplyToCommentCommand, SingleResponse<bool>>
    {
        private readonly HomeDbContext _context;

        public ReplyToCommentCommandHandler(HomeDbContext context)
        {
            _context = context;
        }

        public async Task<SingleResponse<bool>> Handle(ReplyToCommentCommand request, CancellationToken cancellationToken)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);
            if (post == null)
            {
                return new SingleResponse<bool>(false);
            }

            var parentComment = await _context.PostComments
                .FirstOrDefaultAsync(c => c.Id == request.ParentCommentId && c.PostId == request.PostId, cancellationToken);
            if (parentComment == null)
            {
                return new SingleResponse<bool>(false);
            }

            var reply = new PostComment
            {
                PostId = request.PostId,
                ParentCommentId = request.ParentCommentId,
                UserId = request.UserId,
                Content = request.Content
            };

            _context.PostComments.Add(reply);

            await _context.SaveChangesAsync(cancellationToken);

            return new SingleResponse<bool>(true);
        }
    }
}
