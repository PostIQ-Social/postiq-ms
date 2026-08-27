using System;
using System.Collections.Generic;

namespace Home.Core.Entities;

public class PostComment
{
    public long Id { get; set; }
    public long PostId { get; set; }
    public long UserId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public long? ParentCommentId { get; set; }
    public int LikeCount { get; set; } = 0;
    public virtual PostComment ParentComment { get; set; }
    public virtual ICollection<PostComment> Replies { get; set; } = new List<PostComment>();
    public virtual ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
}
