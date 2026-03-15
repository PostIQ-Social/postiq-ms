using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Home.Core.Entities;

[Table("BatchJobStatus", Schema = "Home")]
public partial class BatchJobStatus
{
    [Key]
    public long StatusId { get; set; }

    public Guid BatchId { get; set; }

    public int BatchSize { get; set; }

    public long StartId { get; set; }

    public long LastId { get; set; }

    public int RecordCount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ExecutionStartedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ExecutionEndedAt { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedOn { get; set; }

    public long? CreatedBy { get; set; }
}
