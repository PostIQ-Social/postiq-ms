using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.AI.Configuration
{
    public class AiConfiguration
    {
        public static string SectionName = "AI";
        public string Model { get; set; }
        public string ApiKey { get; set; }
    }
}
