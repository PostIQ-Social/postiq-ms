using Home.Application.Queries;
using Home.Core.Entities;
using Home.Core.Persistence;
using MediatR;
using PostIQ.Core.Database;
using PostIQ.Core.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Application.Handlers
{
    internal class GetPostsCountHandler : IRequestHandler<GetPostCountQuery, ListResponse<PostsCount>>
    {
        private readonly IUnitOfWork<HomeDbContext> _uow;
        private readonly IRepositoryAsync<PostsCount> _postsCountRepository;
        public GetPostsCountHandler(IUnitOfWork<HomeDbContext> uow)
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
