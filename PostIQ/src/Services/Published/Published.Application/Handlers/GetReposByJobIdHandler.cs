using MediatR;
using Microsoft.EntityFrameworkCore;
using PostIQ.Core.Response;
using PostIQ.Core.Shared.Enums;
using Published.Application.Queries;
using Published.Core.Entities;
using Published.Core.Persistence;

namespace Published.Application.Handlers;

public sealed class GetReposByJobIdHandler : IRequestHandler<GetReposByJobIdQuery, ListResponse<Repo>>
{
    private readonly PublishDbContext _context;

    public GetReposByJobIdHandler(PublishDbContext context)
    {
        _context = context;
    }

    public async Task<ListResponse<Repo>> Handle(GetReposByJobIdQuery request, CancellationToken cancellationToken)
    {
        var repos = await _context.Repos
            .AsNoTracking()
            .Where(repo => repo.JobId == request.JobId && repo.Status == (int)StatusEnum.Pending)
            .ToListAsync(cancellationToken);

        return new ListResponse<Repo> { Data = repos };
    }
}
