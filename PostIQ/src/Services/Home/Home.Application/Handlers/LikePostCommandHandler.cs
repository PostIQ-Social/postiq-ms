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
    public class LikePostCommandHandler : IRequestHandler<LikePostCommand, SingleResponse<bool>>
    {
        private readonly HomeDbContext _context;

        public LikePostCommandHandler(HomeDbContext context)
        {
            _context = context;
        }

        public async Task<SingleResponse<bool>> Handle(LikePostCommand request, CancellationToken cancellationToken)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);
            if (post == null)
            {
                return new SingleResponse<bool>(false);
            }

            var existingLike = await _context.PostLikes
                .FirstOrDefaultAsync(l => l.PostId == request.PostId && l.UserId == request.UserId, cancellationToken);

            if (existingLike == null)
            {
                // Add like
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
                // Remove like (toggle)
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
