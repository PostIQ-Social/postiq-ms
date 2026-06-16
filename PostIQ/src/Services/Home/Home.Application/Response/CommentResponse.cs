using System;

namespace Home.Application.Response
{
    public record CommentResponse
    {
        public long Id { get; set; }
        public long PostId { get; set; }
        public long UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedOn { get; set; }
        public int LikeCount { get; set; }
    }
}
