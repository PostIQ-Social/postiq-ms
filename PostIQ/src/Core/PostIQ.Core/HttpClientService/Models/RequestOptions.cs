using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.HttpClientService.Models
{
    public sealed class RequestOptions
    {
        /// <summary>
        /// Additional or override headers for this request.
        /// </summary>
        public Dictionary<string, string>? Headers { get; set; }

        /// <summary>
        /// Request-specific timeout. Overrides client default when set.
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Cancellation token for this request.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Completion option. Use ResponseHeadersRead for large responses to stream immediately.
        ///</summary>
        public HttpCompletionOption CompletionOption { get; set; } = HttpCompletionOption.ResponseContentRead;

        /// <summary>
        /// Optional bearer token. If set, adds Authorization: Bearer (token).
        /// </summary>
        public string? BearerToken { get; set; }

        /// <summary>
        /// optional basic Auth : tuple of (username, password). 
        /// </summary>
        public (string UserName, string Password)? BasicAuth { get; set; }
    }
}
