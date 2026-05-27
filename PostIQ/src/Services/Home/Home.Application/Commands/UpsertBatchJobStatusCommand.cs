using MediatR;
using Microsoft.EntityFrameworkCore;
using PostIQ.Core.Response;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Home.Application.Commands
{
    public record UpsertBatchJobStatusCommand : IRequest<SingleResponse<bool>>
    {
        public long StatusId { get; set; }

        public Guid BatchId {  get; set; }

        public int BatchSize { get; set; }

        public long StartId { get; set; }

        public long LastId { get; set; }

        public int RecordCount { get; set; }

        public DateTime? ExecutionStartedAt { get; set; }

        public DateTime? ExecutionEndedAt { get; set; }

        public string? Status { get; set; }
    }
}
