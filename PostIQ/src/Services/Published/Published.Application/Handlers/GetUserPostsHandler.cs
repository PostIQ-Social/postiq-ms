using AutoMapper;
using MediatR;
using PostIQ.Core.Database;
using PostIQ.Core.Database.Extension;
using PostIQ.Core.Response;
using Published.Application.Queries;
using Published.Application.Response;
using Published.Core.Entities;
using Published.Core.Persistence;

namespace Published.Application.Handlers
{
    public class GetUserPostsHandler : IRequestHandler<GetUserPostsQuery, ListResponse<BatchRepoRes>>
    {
        private readonly IRepositoryAsync<ProcessedPost> _processedPosts;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<PublishDbContext> _uow;

        public GetUserPostsHandler(
            IUnitOfWork<PublishDbContext> uow,
            IMapper mapper)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _processedPosts = _uow.GetRepositoryAsync<ProcessedPost>();
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ListResponse<BatchRepoRes>> Handle(GetUserPostsQuery request, CancellationToken cancellationToken)
        {
            var result = await (from j in _uow.Context.Jobs
                        join r in _uow.Context.Repos on j.JobId equals r.JobId
                        join p in _uow.Context.ProcessedPosts on r.RepoId equals p.RepoId
                        where p.IsActive && j.UserId == request.UserId
                        select p).ToPaginateAsync(request.PageNo, request.PageSize);

            var response = _mapper.Map<ListResponse<BatchRepoRes>>(result);
            
            return response;
        }
    }
}
