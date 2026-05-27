using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.HttpClientService.Models
{
    /// <summary>
    /// Result of an HTTP call with optional stream for large response bodies.
    /// </summary>
    public sealed class HttpResponseResult : IDisposable
    {
        public int StatusCode { get; set; }
        public bool IsSuccessStatusCode { get; set; }
        public string? ReasonPhrase { get; set; }
        public IReadOnlyDictionary<string, string>? Headers { get; set; }

        /// <summary>
        /// Response body as bytes. Only populated when not using stream; for large responses use ResponseStream.
        /// </summary>
        public byte[]? Content { get; set; }

        /// <summary>
        /// Response body as stream. Caller must dispose when using stream. Prefer for large downloads.
        /// </summary>
        public Stream? ResponseStream { get; set; }

        public void Dispose() => ResponseStream?.Dispose();
    }
}
