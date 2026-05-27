using System;
using System.Collections.Generic;
using System.Text;

namespace Published.Application.Models
{
    public record JobModel
    {
        public long Id { get; init; }
        public string? Name { get; init; }
        public string? Payload { get; init; }
        public DateTime? NextExecutionTime { get; init; }
    }
}
