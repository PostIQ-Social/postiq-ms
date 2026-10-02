using MediatR;
using Microsoft.EntityFrameworkCore;
using PostIQ.Core.Response;
using Published.Application.Queries;
using Published.Application.Response;
using Published.Core.Persistence;

namespace Published.Application.Handlers;

public sealed class GetJobsByUserIdHandler : IRequestHandler<GetJobsByUserIdQuery, ListResponse<UserJobResponse>>
{
    private readonly PublishDbContext _context;

    public GetJobsByUserIdHandler(PublishDbContext context)
    {
        _context = context;
    }

    public async Task<ListResponse<UserJobResponse>> Handle(GetJobsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _context.Jobs
            .AsNoTracking()
            .Where(job => job.UserId == request.UserId)
            .Select(job => new UserJobResponse(job.Source, job.BaseUrl))
            .ToListAsync(cancellationToken);

        return new ListResponse<UserJobResponse> { Data = jobs };
    }
}