using MediatR;
using Microsoft.EntityFrameworkCore;
using Published.Application.Queries;
using Published.Core.Persistence;

namespace Published.Application.Handlers;

public sealed class GetLikedPostIdsHandler : IRequestHandler<GetLikedPostIdsQuery, List<long>>
{
    private readonly PublishDbContext _context;

    public GetLikedPostIdsHandler(PublishDbContext context)
    {
        _context = context;
    }

    public Task<List<long>> Handle(GetLikedPostIdsQuery request, CancellationToken cancellationToken) =>
        _context.PostLikes
            .AsNoTracking()
            .Where(like => like.UserId == request.UserId && request.PostIds.Contains(like.PostId))
            .Select(like => like.PostId)
            .ToListAsync(cancellationToken);
}