using System;

namespace User.Application.Response
{
    public class PostResponse
    {
        public long PostId { get; set; }
        public long UserId { get; set; }
        public string Source { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Link { get; set; } = null!;
        public string? Content { get; set; }
        public DateTime? PublishedDate { get; set; }
        public string? Categories { get; set; }
    }
}
