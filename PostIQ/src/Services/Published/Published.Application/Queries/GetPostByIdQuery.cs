using MediatR;
using Published.Application.Response;

namespace Published.Application.Queries;

public record GetPostByIdQuery(long PostId) : IRequest<BatchRepoRes?>;