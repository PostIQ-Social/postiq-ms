using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PostIQ.Core.Database;
using PostIQ.Core.Response;
using Published.Application.Queries;
using Published.Application.Response;
using Published.Core.Entities;
using Published.Core.Persistence;
using System.Linq.Expressions;

namespace Published.Application.Handlers;

public class SearchPostsHandler : IRequestHandler<SearchPostsQuery, ListResponse<BatchRepoRes>>
{
    private readonly IRepositoryAsync<ProcessedPost> _processedPosts;
    private readonly IMapper _mapper;

    public SearchPostsHandler(IUnitOfWork<PublishDbContext> unitOfWork, IMapper mapper)
    {
        _processedPosts = unitOfWork.GetRepositoryAsync<ProcessedPost>();
        _mapper = mapper;
    }

    public async Task<ListResponse<BatchRepoRes>> Handle(SearchPostsQuery request, CancellationToken cancellationToken)
    {
        var term = request.Query.Trim().ToLowerInvariant();
        if (term.Length == 0)
        {
            return new ListResponse<BatchRepoRes> { Data = new List<BatchRepoRes>() };
        }

        Expression<Func<ProcessedPost, bool>> matches = request.SearchBy.ToLowerInvariant() switch
        {
            "author" => post => post.IsActive && post.OriginalAuthor != null && post.OriginalAuthor.ToLower().Contains(term),
            "title" => post => post.IsActive && ((post.Headline != null && post.Headline.ToLower().Contains(term))
                || (post.OriginalTitle != null && post.OriginalTitle.ToLower().Contains(term))),
            "source" => post => post.IsActive && (post.Repo.Source.ToLower().Contains(term)
                || post.Repo.RepoUrl.ToLower().Contains(term)),
            _ => post => post.IsActive && ((post.OriginalAuthor != null && post.OriginalAuthor.ToLower().Contains(term))
                || (post.Headline != null && post.Headline.ToLower().Contains(term))
                || (post.OriginalTitle != null && post.OriginalTitle.ToLower().Contains(term))
                || (post.Summary != null && post.Summary.ToLower().Contains(term))
                || (post.Takeaways != null && post.Takeaways.ToLower().Contains(term))
                || (post.Hashtags != null && post.Hashtags.ToLower().Contains(term))
                || post.Repo.Source.ToLower().Contains(term)
                || post.Repo.RepoUrl.ToLower().Contains(term))
        };

        var page = Math.Max(1, request.PageNo);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var result = await _processedPosts.GetListAsync(
            predicate: matches,
            orderBy: query => query.OrderByDescending(post => post.CreatedOn),
            include: query => query.Include(post => post.Repo).ThenInclude(repo => repo.Job),
            index: page - 1,
            size: pageSize,
            enableTracking: false,
            cancellationToken: cancellationToken);

        return _mapper.Map<ListResponse<BatchRepoRes>>(result);
    }
}