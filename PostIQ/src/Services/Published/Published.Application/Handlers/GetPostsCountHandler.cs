using MediatR;
using PostIQ.Core.Database;
using PostIQ.Core.Response;
using Published.Application.Queries;
using Published.Core.Entities;
using Published.Core.Persistence;
using System;
using System.Linq;

namespace Published.Application.Handlers
{
    internal class GetPostsCountHandler : IRequestHandler<GetPostCountQuery, ListResponse<PostsCount>>
    {
        private readonly IUnitOfWork<PublishDbContext> _uow;
        private readonly IRepositoryAsync<PostsCount> _postsCountRepository;

        public GetPostsCountHandler(IUnitOfWork<PublishDbContext> uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _postsCountRepository = _uow.GetRepositoryAsync<PostsCount>();
        }

        public async Task<ListResponse<PostsCount>> Handle(GetPostCountQuery request, CancellationToken cancellationToken)
        {
            var response = new ListResponse<PostsCount>();
            var postsCount = await _postsCountRepository.GetListAsync(x => request.PostId.Contains(x.PostId));
            response.Data = postsCount.Data.ToList();
            return response;
        }
    }
}
