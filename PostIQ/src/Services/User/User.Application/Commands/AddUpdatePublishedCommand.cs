using MediatR;
using PostIQ.Core.Response;
using System.ComponentModel.DataAnnotations;

namespace User.Application.Commands
{
    public class AddUpdatePublishedCommand : IRequest<SingleResponse<long>>
    {
        [Required]
        public long UserId { get; set; }

        [Required]
        public string Source { get; set; } = null!;

        [Required]
        public string BaseUrl { get; set; } = null!;
    }
}
