using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Application.Response
{
    public record BatchPostResponse
    {
        public long UserId { get; init; }

        public long RepoDetailsId { get; init; }

        public string? Source { get; init; }

        public string? RepoUrl { get; init; }

        public string? Key { get; init; }

        public string? Value { get; init; }

        public int? Ordered { get; init; }

        public string IsActive { get; init; } = null!;

        public DateTime PostedOn { get; init; }
    }
}
