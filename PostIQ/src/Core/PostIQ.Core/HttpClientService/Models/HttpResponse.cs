using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.HttpClientService.Models
{
    // <summary>
    /// Typed HTTP response: status, headers, and deserialized body of type <typeparamref name="T"/>
    /// </summary>
    public sealed class HttpResponse<T>
    {
        public bool IsSuccessStatusCode { get; init; }
        public int StatusCode { get; init; }
        public string? ReasonPhrase { get; init; }
        public IReadOnlyDictionary<string, string>? Headers { get; init; }
        /// <summary>
        /// Deserialized response body. Default when status is not success or body is empty.
        /// </summary>
        public T? Value { get; init; }
        /// <summary>
        /// Raw response body as UTF-8 string, when available (e.g. for error inspection).
        /// </summary>
        public string? RawBody { get; init; }
        /// <summary>
        /// Throws if IsSuccessStatusCode is false. Returns this for chaining.
        /// </summary>
        public HttpResponse<T> EnsureSuccessStatusCode()
        {
            if (!IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Response status code does not indicate success: {(int)StatusCode} ({ReasonPhrase}). Body: {RawBody}");
            }
            return this;
        }
    }
}
