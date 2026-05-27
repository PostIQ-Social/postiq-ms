using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace User.Core.Entities;

public partial class Post
{
    [Key]
    public long PostId { get; set; }

    public long UserId { get; set; }

    public string ExternalId { get; set; } = null!;

    public string Source { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Link { get; set; } = null!;

    public string? Content { get; set; }

    public DateTime PublishedDate { get; set; }

    public string? Categories { get; set; }

    public DateTime CreatedOn { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public long? UpdatedBy { get; set; }

    [ForeignKey("UserId")]
    public virtual Users User { get; set; } = null!;
}
