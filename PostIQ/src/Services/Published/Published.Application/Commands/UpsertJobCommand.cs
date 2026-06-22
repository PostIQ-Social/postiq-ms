using MediatR;
using PostIQ.Core.Response;

namespace Published.Application.Commands
{
    public class UpsertJobCommand : IRequest<SingleResponse<long>>
    {
        public long PublishedId { get; set; }

        public long UserId { get; set; }

        public string Source { get; set; } = null!;

        public string BaseUrl { get; set; } = null!;
    }
}
