using MediatR;

namespace Published.Application.Queries;

public sealed record GetLikedPostIdsQuery(long UserId, long[] PostIds) : IRequest<List<long>>;