using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.HttpClientService.Configuration
{
    public sealed class HttpClientOptions
    {
        public string? BaseAddress { get; set; }

        /// <summary>
        /// Request timeout. For large files use high value or null for no timeout (default: 2 hours).
        /// </summary>
        public TimeSpan? Timeout { get; set; } = TimeSpan.FromHours(2);

        /// <summary>
        /// Timeout for the overall operation when using HttpClient.SendAsync (optional override).
        /// </summary>
        public TimeSpan? OverallTimeout { get; set; }

        /// <summary>
        /// Default request headers. Key = header name, Value = header value.
        /// </summary>
        public Dictionary<string, string> DefaultHeaders { get; set; } = new();

        /// <summary>
        /// Max buffer size when copying streams (default 81920). Increase for faster large file I/O.
        /// </summary>
        public int BufferSize { get; set; } = 81_920;

        /// <summary>
        /// Max response content buffer size in bytes. Zero or null = do not buffer (stream only).
        /// For large responses keep 0 to avoid OOM.
        /// </summary>
        public long? MaxResponseContentBufferSize { get; set; } = 0;

        /// <summary>
        /// Use HTTP/2 if available.
        /// </summary>
        public bool UseHttp2 { get; set; } = false;

        /// <summary>
        /// Automatic decompression (GZip, Deflate, Brotli).
        /// </summary>
        public bool AutomaticDecompression { get; set; } = true;

        /// <summary>
        /// Number of retries on transient failure (0 = no retry).
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Delay between retries.
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);
    }
}
