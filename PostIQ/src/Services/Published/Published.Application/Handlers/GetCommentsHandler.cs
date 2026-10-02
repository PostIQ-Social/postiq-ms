using MediatR;
using Microsoft.EntityFrameworkCore;
using PostIQ.Core.Database;
using PostIQ.Core.Response;
using Published.Application.Queries;
using Published.Application.Response;
using Published.Core.Entities;
using Published.Core.Persistence;

namespace Published.Application.Handlers
{
    public class GetCommentsHandler : IRequestHandler<GetCommentsQuery, ListResponse<CommentResponse>>
    {
        private readonly IRepositoryAsync<PostComment> _comments;

        public GetCommentsHandler(IUnitOfWork<PublishDbContext> uow)
        {
            _comments = uow.GetRepositoryAsync<PostComment>();
        }

        public async Task<ListResponse<CommentResponse>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
        {
            var result = await _comments.GetListAsync(
                predicate: x => x.PostId == request.PostId,
                orderBy: q => q.OrderBy(c => c.CreatedOn),
                include: i => i.Include(c => c.Likes),
                enableTracking: false);

            var response = new ListResponse<CommentResponse>
            {
                Data = result.Data.Select(c => new CommentResponse
                {
                    Id = c.Id,
                    PostId = c.PostId,
                    UserId = c.UserId,
                    Content = c.Content,
                    CreatedOn = c.CreatedOn,
                    LikeCount = c.LikeCount
                }).ToList()
            };

            return response;
        }
    }
}
