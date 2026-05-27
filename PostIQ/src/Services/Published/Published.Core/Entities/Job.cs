using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Published.Core.Entities;

[Table("Job", Schema = "Published")]
public partial class Job
{
    [Key]
    public long JobId { get; set; }

    public long PublishedId { get; set; }

    public long UserId { get; set; }

    [StringLength(50)]
    public string Source { get; set; } = null!;

    [StringLength(100)]
    public string BaseUrl { get; set; } = null!;

    [Unicode(false)]
    public bool IsActive { get; set; } = true;

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    public long CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedOn { get; set; }

    public long? UpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ExecutionStartTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NextExecutionTime { get; set; }


    [InverseProperty("Job")]
    public virtual ICollection<Repo> Repos { get; set; } = new List<Repo>();
}
