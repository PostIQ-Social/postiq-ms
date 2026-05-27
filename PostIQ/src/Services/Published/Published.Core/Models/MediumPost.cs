using System;
using System.Collections.Generic;
using System.Text;

namespace Published.Core.Models
{
    public class MediumPost
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public DateTimeOffset PublishedAt { get; set; }
        public string Author { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }

}
