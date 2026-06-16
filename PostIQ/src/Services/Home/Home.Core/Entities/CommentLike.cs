using System;

namespace Home.Core.Entities;

public class CommentLike
{
    public long Id { get; set; }
    public long CommentId { get; set; }
    public long UserId { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public virtual PostComment Comment { get; set; }
}
