using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.HttpClientService.Models
{
    /// <summary>
    /// Represents a single file or stream to upload (for single or multi-file upload).
    /// </summary>
    public sealed class FileUploadItem
    {
        /// <summary>
        /// Form field name (e.g. "file" or "files"). For multiple files, same name is typically used.
        /// </summary>
        public string FormFieldName { get; set; } = "file";

        /// <summary>
        /// File name to send in Content-Disposition (e.g. "document.pdf").
        /// </summary>
        public string FileName { get; set; } = "file";

        /// <summary>
        /// Content type (e.g. "application/pdf"). Null let implementation infer or use application/octet-stream.
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// Stream containing file data. Must be readable and support Length/Seek for length if possible.
        /// </summary>
        /// <remarks>
        /// For very large files, use a stream that supports reading without loading into memory.
        /// </remarks>
        public Stream Content { get; set; } = null!;
    }
}
