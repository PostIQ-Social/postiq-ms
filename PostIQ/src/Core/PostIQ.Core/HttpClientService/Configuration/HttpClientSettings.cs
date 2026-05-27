using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.HttpClientService.Configuration
{
    /// <summary>
    /// Root section for multiple named HTTP client configurations in appsettings.json.
    /// </summary>
    public sealed class HttpClientSettings
    {
        public const string SectionName = "HttpClientSettings";

        /// <summary>
        /// Named client configurations. Key client name used when resolving the client.
        /// </summary>
        public Dictionary<string, HttpClientOptions> Clients { get; set; } = new();
    }
}
