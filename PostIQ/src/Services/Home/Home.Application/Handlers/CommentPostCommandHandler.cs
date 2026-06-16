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
    public class CommentPostCommandHandler : IRequestHandler<CommentPostCommand, SingleResponse<bool>>
    {
        private readonly HomeDbContext _context;

        public CommentPostCommandHandler(HomeDbContext context)
        {
            _context = context;
        }

        public async Task<SingleResponse<bool>> Handle(CommentPostCommand request, CancellationToken cancellationToken)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);
            if (post == null)
            {
                return new SingleResponse<bool>(false);
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
