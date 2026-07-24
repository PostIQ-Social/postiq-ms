using MediatR;
using PostIQ.Core.Response;
using Published.Application.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Published.Application.Commands
{
    public class AddJobCommand : IRequest<CreatedResponse<long>>
    {
        public long PublishedId { get; set; }

        public long UserId { get; set; }

        [StringLength(50)]
        public string Source { get; set; } = null!;

        [StringLength(100)]
        public string BaseUrl { get; set; } = null!;
    }
}
