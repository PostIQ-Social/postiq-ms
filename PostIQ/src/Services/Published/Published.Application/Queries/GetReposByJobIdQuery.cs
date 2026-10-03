using MediatR;
using PostIQ.Core.Response;
using Published.Core.Entities;

namespace Published.Application.Queries;

public record GetReposByJobIdQuery(long JobId) : IRequest<ListResponse<Repo>>;
