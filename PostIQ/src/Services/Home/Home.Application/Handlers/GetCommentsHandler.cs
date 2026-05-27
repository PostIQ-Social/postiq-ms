using Home.Application.Queries;
using Home.Application.Response;
using Home.Core.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PostIQ.Core.Response;

namespace Home.Application.Handlers
{
    public class GetCommentsHandler : IRequestHandler<GetCommentsQuery, ListResponse<CommentResponse>>
    {
        private readonly HomeDbContext _context;

        public GetCommentsHandler(HomeDbContext context)
        {
            _context = context;
        }

        public async Task<ListResponse<CommentResponse>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
        {
            var comments = await _context.PostComments
                .Where(c => c.PostId == request.PostId && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedOn)
                .Select(c => new CommentResponse
                {
                    Id = c.Id,
                    PostId = c.PostId,
                    UserId = c.UserId,
                    Content = c.Content,
                    CreatedOn = c.CreatedOn,
                    LikeCount = c.LikeCount
                })
                .ToListAsync(cancellationToken);

            return new ListResponse<CommentResponse>(comments)
            {
                Count = comments.Count
            };
        }
    }
}
