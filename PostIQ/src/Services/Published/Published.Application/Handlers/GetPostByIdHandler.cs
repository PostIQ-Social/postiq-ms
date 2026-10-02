using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PostIQ.Core.Database;
using Published.Application.Queries;
using Published.Application.Response;
using Published.Core.Entities;
using Published.Core.Persistence;

namespace Published.Application.Handlers;

public class GetPostByIdHandler : IRequestHandler<GetPostByIdQuery, BatchRepoRes?>
{
    private readonly IRepositoryAsync<ProcessedPost> _processedPosts;
    private readonly IMapper _mapper;

    public GetPostByIdHandler(IUnitOfWork<PublishDbContext> unitOfWork, IMapper mapper)
    {
        _processedPosts = unitOfWork.GetRepositoryAsync<ProcessedPost>();
        _mapper = mapper;
    }

    public async Task<BatchRepoRes?> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await _processedPosts.SingleOrDefaultAsync(
            predicate: item => item.ProcessedPostId == request.PostId && item.IsActive,
            orderBy: null,
            include: query => query.Include(item => item.Repo).ThenInclude(repo => repo.Job),
            enableTracking: false,
            ignoreQueryFilters: false);

        return post is null ? null : _mapper.Map<BatchRepoRes>(post);
    }
}