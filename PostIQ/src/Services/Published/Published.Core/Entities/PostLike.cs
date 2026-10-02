using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Published.Core.Entities;

public class PostLike
{
    public long Id { get; set; }
    public long PostId { get; set; }
    public long UserId { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

}
