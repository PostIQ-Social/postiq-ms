using System;
using System.Collections.Generic;
using System.Text;

namespace Home.Core.Entities
{
    public class PostsCount
    {
        public long CountId { get; set; }
        public long PostId { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
    }
}
