using System;

namespace User.Core.Entities;

public partial class Published
{
    public long PublishedId { get; set; }

    public long UserId { get; set; }

    public string? Source { get; set; }

    public string? BaseUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public long? UpdatedBy { get; set; }

}
