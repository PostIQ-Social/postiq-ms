using MediatR;
using Published.Application.Commands;
using Published.Core.Persistence;
using Published.Core.Entities;
using Microsoft.EntityFrameworkCore;
using PostIQ.Core.Response;

namespace Published.Application.Handlers
{
    public class LikePostCommandHandler : IRequestHandler<LikePostCommand, SingleResponse<bool>>
    {
        private readonly PublishDbContext _context;

        public LikePostCommandHandler(PublishDbContext context)
        {
            _context = context;
        }

        public async Task<SingleResponse<bool>> Handle(LikePostCommand request, CancellationToken cancellationToken)
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

            var existingLike = await _context.PostLikes
                .FirstOrDefaultAsync(l => l.PostId == request.PostId && l.UserId == request.UserId, cancellationToken);

            if (existingLike == null)
            {
                var like = new PostLike
                {
                    PostId = request.PostId,
                    UserId = request.UserId
                };
                _context.PostLikes.Add(like);
                post.LikeCount++;
            }
            else
            {
                _context.PostLikes.Remove(existingLike);
                if (post.LikeCount > 0)
                {
                    post.LikeCount--;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new SingleResponse<bool>(true);
        }
    }
}
