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
    public class GetBatchReposHandler : IRequestHandler<GetBatchReposQuery, ListResponse<BatchRepoRes>>
    {
        private readonly IRepositoryAsync<ProcessedPost> _processedPosts;
        private readonly IMapper _mapper;

        public GetBatchReposHandler(
            IUnitOfWork<PublishDbContext> uow,
            IMapper mapper)
        {
            _processedPosts = uow.GetRepositoryAsync<ProcessedPost>();
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ListResponse<BatchRepoRes>> Handle(GetBatchReposQuery request, CancellationToken cancellationToken)
        {
            var result = await _processedPosts.GetListAsync(
                predicate: x => x.ProcessedPostId > request.AfterId && x.IsActive,
                orderBy: o => o.OrderBy(x => x.ProcessedPostId),
                include: i => i.Include(p => p.Repo)
                                .ThenInclude(r => r.Job),
                index: -1, 
                size: request.BatchSize,
                enableTracking: false);

           var response = _mapper.Map<ListResponse<BatchRepoRes>>(result);

            return response;
        }
    }
}
