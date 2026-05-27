using System;
using System.Collections.Generic;

namespace User.Core.Entities;

public partial class UserDetail
{
    public long UserDetailId { get; set; }

    public long UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string LastName { get; set; } = null!;

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual Users UserDetailNavigation { get; set; } = null!;
}
