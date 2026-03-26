using AutoMapper;
using Home.Application.Commands;
using Home.Core.Entities;
using Home.Core.Persistence;
using MediatR;
using PostIQ.Core.Database;
using PostIQ.Core.Response;

namespace Home.Application.Handlers
{
    public class MergePostHandler : IRequestHandler<MergePostCommand, SingleResponse<bool>>
    {
        private readonly IRepositoryAsync<Post> _postAsync;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<HomeDbContext> _uow;

        public MergePostHandler(IUnitOfWork<HomeDbContext> uow, IMapper mapper)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _postAsync = _uow.GetRepositoryAsync<Post>();
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<SingleResponse<bool>> Handle(MergePostCommand request, CancellationToken cancellationToken)
        {
            var response = new SingleResponse<bool>(false);
            if (request.Models.Any())
            {

                var repoDetailIds = request.Models.Select(x => x.ProcessedPostId).Distinct();
                //var _post = _uow.GetRepository<Post>();
                //var posts = _post.GetList(x => repoDetailIds.Contains(x.ProcessedPostId), index: -1, size: -1);
                //if (posts.Count > 0)
                //{
                //    posts.Data.ToList().ForEach(x => { x.IsActive = false; x.UpdatedOn = DateTime.UtcNow; x.UpdatedBy = x.CreatedBy; });
                //    _uow.GetRepository<Post>().Update(posts.Data);
                //    var saved = await _uow.CommitAsync().ConfigureAwait(false);
                //    response.Data = saved > 0;
                //}

                var entity = _mapper.Map<List<Post>>(request.Models);
                entity.ForEach(x => { x.CreatedOn = DateTime.UtcNow; x.IsActive = true; });
                await _postAsync.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
                var inserted = await _uow.CommitAsync();
                response.Data = inserted > 0;
            }
            return response;
        }
    }
}
