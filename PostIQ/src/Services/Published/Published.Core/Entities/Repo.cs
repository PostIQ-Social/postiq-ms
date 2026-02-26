using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Published.Core.Entities;

[Table("Repos", Schema = "Published")]
public partial class Repo
{
    [Key]
    public long RepoId { get; set; }

    public long JobId { get; set; }

    public long PublishedId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Source { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string RepoUrl { get; set; } = null!;

    public int Status { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime PostedOn { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    public long CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedOn { get; set; }

    public long? UpdatedBy { get; set; }

    public string MetaData { get; set; } = null!;

    [ForeignKey("JobId")]
    [InverseProperty("Repos")]
    public virtual Job Job { get; set; } = null!;

    [InverseProperty("Repo")]
    public virtual ICollection<RepoDetail> RepoDetails { get; set; } = new List<RepoDetail>();
}
