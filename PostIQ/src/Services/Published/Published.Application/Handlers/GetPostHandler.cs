using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PostIQ.Core.Database;
using PostIQ.Core.Response;
using Published.Application.Queries;
using Published.Application.Response;
using Published.Application.Services;
using Published.Core.Entities;
using Published.Core.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Published.Application.Handlers
{
    public class GetPostHandler : IRequestHandler<GetPostQuery, ListResponse<BatchRepoRes>>
    {
        private readonly IRepositoryAsync<ProcessedPost> _processedPosts;
        private readonly IMapper _mapper;

        public GetPostHandler(
            IUnitOfWork<PublishDbContext> uow,
            IMapper mapper)
        {
            _processedPosts = uow.GetRepositoryAsync<ProcessedPost>();
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ListResponse<BatchRepoRes>> Handle(GetPostQuery request, CancellationToken cancellationToken)
        {
            var result = await _processedPosts.GetListAsync(
                predicate: x => x.IsActive,
                orderBy: o => o
                    .OrderByDescending(x => x.PostsCount == null ? 0 : x.PostsCount.LikeCount)
                    .ThenByDescending(x => x.PostsCount == null ? 0 : x.PostsCount.CommentCount)
                    .ThenBy(x => x.ProcessedPostId),
                include: i => i.Include(p => p.Repo)
                                .ThenInclude(repo => repo.Job)
                                .Include(p => p.PostsCount),
                index: request.PageNo - 1,
                size: request.PageSize,
                enableTracking: false);

           var response = _mapper.Map<ListResponse<BatchRepoRes>>(result);

            return response;
        }
    }
}
