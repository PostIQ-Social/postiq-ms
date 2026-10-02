using MediatR;
using Published.Application.Commands;
using Published.Core.Persistence;
using Published.Core.Entities;
using Microsoft.EntityFrameworkCore;
using PostIQ.Core.Response;

namespace Published.Application.Handlers
{
    public class CommentPostCommandHandler : IRequestHandler<CommentPostCommand, SingleResponse<bool>>
    {
        private readonly PublishDbContext _context;

        public CommentPostCommandHandler(PublishDbContext context)
        {
            _context = context;
        }

        public async Task<SingleResponse<bool>> Handle(CommentPostCommand request, CancellationToken cancellationToken)
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

            var comment = new PostComment
            {
                PostId = request.PostId,
                UserId = request.UserId,
                Content = request.Content
            };

            _context.PostComments.Add(comment);
            post.CommentCount++;

            await _context.SaveChangesAsync(cancellationToken);

            return new SingleResponse<bool>(true);
        }
    }
}
