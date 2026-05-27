using MediatR;
using System.ComponentModel.DataAnnotations;

namespace User.Application.Commands
{
    public class SyncContentCommand : IRequest<bool>
    {
        [Required]
        public long UserId { get; set; }

        [Required]
        public string Source { get; set; } = null!;

        [Required]
        public string BaseUrl { get; set; } = null!;
    }
}
