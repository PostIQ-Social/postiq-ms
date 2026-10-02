using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Published.Core.Entities;

[Table("PostsCount", Schema = "Published")]
public class PostsCount
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long CountId { get; set; }

    // FK to Published.ProcessedPosts.ProcessedPostId
    public long PostId { get; set; }

    public int LikeCount { get; set; }
    public int CommentCount { get; set; }

    [ForeignKey(nameof(PostId))]
    [InverseProperty(nameof(ProcessedPost.PostsCount))]
    public virtual ProcessedPost Post { get; set; } = null!;
}
