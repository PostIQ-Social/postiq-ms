using AutoMapper;
using Home.Application.Queries;
using Home.Application.Response;
using Home.Core.Entities;
using Home.Core.Persistence;
using MediatR;
using PostIQ.Core.Database;
using PostIQ.Core.Response;

namespace Home.Application.Handlers
{
    public class GetPostsHandler : IRequestHandler<GetPostsQuery, ListResponse<PostResponse>>
    {
        private readonly IRepositoryAsync<Post> _post;
        private readonly IMapper _mapper;

        public GetPostsHandler(IUnitOfWork<HomeDbContext> uow, IMapper mapper)
        {
            _post = uow.GetRepositoryAsync<Post>();
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ListResponse<PostResponse>> Handle(GetPostsQuery request, CancellationToken cancellationToken)
        {
            var response = new ListResponse<PostResponse>();
            var result  = await _post.GetListAsync(predicate: x => x.IsActive == true,
                                                index: request.pageNo -1 , 
                                                size: (int)request.pageSize, 
                                                cancellationToken: cancellationToken);
            response.Data = _mapper.Map<List<PostResponse>>(result.Data);
            response.Count = result.Count;
            response.TotalPages = result.Pages;
            return response;
        }
    }
}
