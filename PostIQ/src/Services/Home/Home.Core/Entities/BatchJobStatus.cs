using System;
using System.Collections.Generic;

namespace Home.Core.Entities;

public partial class BatchJobStatus
{
    public long StatusId { get; set; }

    public Guid BatchId { get; set; }

    public int BatchSize { get; set; }

    public long StartId { get; set; }

    public long LastId { get; set; }

    public int RecordCount { get; set; }

    public DateTime? ExecutionStartedAt { get; set; }

    public DateTime? ExecutionEndedAt { get; set; }

    public string? Status { get; set; }

    public DateTime CreatedOn { get; set; }

    public long CreatedBy { get; set; }
}
