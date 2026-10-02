using MediatR;
using PostIQ.Core.Response;
using Published.Application.Response;

namespace Published.Application.Queries;

public record SearchPostsQuery(string Query, string SearchBy, int PageNo, int PageSize)
    : IRequest<ListResponse<BatchRepoRes>>;