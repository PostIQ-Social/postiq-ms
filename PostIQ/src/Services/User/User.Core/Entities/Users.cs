using System;
using System.Collections.Generic;

namespace User.Core.Entities;

public partial class Users
{
    public long UserId { get; set; }

    public Guid Guid { get; set; }

    public string Email { get; set; } = null!;

    public string? Otp { get; set; }

    public DateTime? OtpexpireOn { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedOn { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public long? UpdatedBy { get; set; }

    public string? IpAddress { get; set; }

    public virtual ICollection<Published> Publisheds { get; set; } = new List<Published>();

    public virtual UserDetail? UserDetail { get; set; }
}
