using MediatR;
using Published.Application.Commands;
using Published.Core.Persistence;
using Published.Core.Entities;
using Microsoft.EntityFrameworkCore;
using PostIQ.Core.Response;

namespace Published.Application.Handlers
{
    public class ReplyToCommentCommandHandler : IRequestHandler<ReplyToCommentCommand, SingleResponse<bool>>
    {
        private readonly PublishDbContext _context;

        public ReplyToCommentCommandHandler(PublishDbContext context)
        {
            _context = context;
        }

        public async Task<SingleResponse<bool>> Handle(ReplyToCommentCommand request, CancellationToken cancellationToken)
        {
            var post = await _context.PostsCount.FirstOrDefaultAsync(p => p.PostId == request.PostId, cancellationToken);
            if (post == null)
            {
                post = new PostsCount
                {
                    PostId = request.PostId,
                    LikeCount = 0,
                    CommentCount = 0
                };
                _context.PostsCount.Add(post);
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
            post.CommentCount++;

            await _context.SaveChangesAsync(cancellationToken);

            return new SingleResponse<bool>(true);
        }
    }
}
