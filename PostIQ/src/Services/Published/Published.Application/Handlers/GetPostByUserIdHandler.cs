using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PostIQ.Core.Database;
using PostIQ.Core.Response;
using Published.Application.Queries;
using Published.Application.Response;
using Published.Core.Entities;
using Published.Core.Persistence;

namespace Published.Application.Handlers;

public sealed class GetPostByUserIdHandler : IRequestHandler<GetPostByUserIdQuery, ListResponse<BatchRepoRes>>
{
    private readonly IRepositoryAsync<ProcessedPost> _processedPosts;
    private readonly IMapper _mapper;

    public GetPostByUserIdHandler(IUnitOfWork<PublishDbContext> unitOfWork, IMapper mapper)
    {
        _processedPosts = unitOfWork.GetRepositoryAsync<ProcessedPost>();
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ListResponse<BatchRepoRes>> Handle(GetPostByUserIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _processedPosts.GetListAsync(
            predicate: post => post.IsActive && post.Repo.Job.UserId == request.UserId,
            orderBy: posts => posts
                .OrderByDescending(post => post.PostsCount == null ? 0 : post.PostsCount.LikeCount)
                .ThenByDescending(post => post.PostsCount == null ? 0 : post.PostsCount.CommentCount)
                .ThenBy(post => post.ProcessedPostId),
            include: posts => posts.Include(post => post.Repo)
                .ThenInclude(repo => repo.Job)
                .Include(post => post.PostsCount),
            index: request.PageNo - 1,
            size: request.PageSize,
            enableTracking: false,
            cancellationToken: cancellationToken);

        return _mapper.Map<ListResponse<BatchRepoRes>>(result);
    }
}