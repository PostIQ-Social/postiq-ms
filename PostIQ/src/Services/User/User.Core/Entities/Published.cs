using System;
using System.Collections.Generic;

namespace User.Core.Entities;

public partial class Published
{
    public long PublishedId { get; set; }

    public long UserId { get; set; }

    public string? Source { get; set; }

    public string? BaseUrl { get; set; }

    public string IsActive { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public long? UpdatedBy { get; set; }

    public virtual Users User { get; set; } = null!;
}
