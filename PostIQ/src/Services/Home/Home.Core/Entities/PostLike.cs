using System;

namespace Home.Core.Entities;

public class PostLike
{
    public long Id { get; set; }
    public long PostId { get; set; }
    public long UserId { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public virtual Post Post { get; set; }
}
