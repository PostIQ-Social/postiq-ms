using MediatR;

namespace User.Application.Queries;

public sealed record GetUserDisplayNamesQuery(long[] UserIds) : IRequest<Dictionary<long, string>>;