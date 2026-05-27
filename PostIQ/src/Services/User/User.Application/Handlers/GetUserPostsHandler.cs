using MediatR;
using Microsoft.EntityFrameworkCore;
using PostIQ.Core.Database;
using User.Application.Queries;
using User.Application.Response;
using User.Core.Entities;
using User.Core.Persistence;

namespace User.Application.Handlers
{
    public class GetUserPostsHandler : IRequestHandler<GetUserPostsQuery, List<PostResponse>>
    {
        private readonly IUnitOfWork<UserDBContext> _unitOfWork;

        public GetUserPostsHandler(IUnitOfWork<UserDBContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<PostResponse>> Handle(GetUserPostsQuery request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.GetRepositoryAsync<Post>();
            var posts = await repo.GetListAsync(
                predicate: p => p.UserId == request.UserId,
                orderBy: q => q.OrderByDescending(p => p.PublishedDate),
                cancellationToken: cancellationToken
            );

            return posts.Data.Select(p => new PostResponse
            {
                PostId = p.PostId,
                UserId = p.UserId,
                Source = p.Source,
                Title = p.Title,
                Link = p.Link,
                Content = p.Content,
                PublishedDate = p.PublishedDate,
                Categories = p.Categories
            }).ToList();
        }
    }
}
