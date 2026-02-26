using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Published.Core.Entities;

[Table("RepoDetails", Schema = "Published")]
public partial class RepoDetail
{
    [Key]
    public long RepoDetailsId { get; set; }

    public long RepoId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Key { get; set; }

    [Unicode(true)]
    public string? Value { get; set; }

    public int? Ordered { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedOn { get; set; }

    public long CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedOn { get; set; }

    public long? UpdatedBy { get; set; }

    [ForeignKey("RepoId")]
    [InverseProperty("RepoDetails")]
    public virtual Repo Repo { get; set; } = null!;
}
