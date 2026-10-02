using MediatR;
using PostIQ.Core.Response;
using Published.Application.Response;

namespace Published.Application.Queries;

public record GetPostByUserIdQuery : IRequest<ListResponse<BatchRepoRes>>
{
    public long UserId { get; init; }
    public int PageNo { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}