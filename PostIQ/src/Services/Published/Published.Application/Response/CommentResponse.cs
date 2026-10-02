using System;

namespace Published.Application.Response
{
    public record CommentResponse
    {
        public long Id { get; set; }
        public long PostId { get; set; }
        public long UserId { get; set; }
        public long? ParentCommentId { get; set; }
        public string? AuthorName { get; set; }
        public string Content { get; set; }
        public DateTime CreatedOn { get; set; }
        public int LikeCount { get; set; }
        public int ReplyCount { get; set; }
        public bool IsLiked { get; set; }
    }
}
