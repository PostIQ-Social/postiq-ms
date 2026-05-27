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
    public class LikeCommentCommandHandler : IRequestHandler<LikeCommentCommand, SingleResponse<bool>>
    {
        private readonly HomeDbContext _context;

        public LikeCommentCommandHandler(HomeDbContext context)
        {
            _context = context;
        }

        public async Task<SingleResponse<bool>> Handle(LikeCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await _context.PostComments.FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);
            if (comment == null)
            {
                return new SingleResponse<bool>(false);
            }

            var existingLike = await _context.CommentLikes
                .FirstOrDefaultAsync(l => l.CommentId == request.CommentId && l.UserId == request.UserId, cancellationToken);

            if (existingLike == null)
            {
                // Add like
                var like = new CommentLike
                {
                    CommentId = request.CommentId,
                    UserId = request.UserId
                };
                _context.CommentLikes.Add(like);
                comment.LikeCount++;
            }
            else
            {
                // Remove like (toggle)
                _context.CommentLikes.Remove(existingLike);
                if (comment.LikeCount > 0)
                {
                    comment.LikeCount--;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new SingleResponse<bool>(true);
        }
    }
}
