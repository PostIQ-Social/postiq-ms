using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.HttpClientService.Models
{
    // <summary>
    /// Result of a file download (path to saved file or stream).
    /// </summary>
    public sealed class DownloadResult : IDisposable
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string? LocalFilePath { get; set; }
        public string? FileNameFromHeader { get; set; }
        public long BytesWritten { get; set; }

        /// <summary>
        /// When download is to stream, this is set and caller must dispose.
        /// </summary>
        public Stream? ResponseStream { get; set; }

        public void Dispose() => ResponseStream?.Dispose();
    }
}
