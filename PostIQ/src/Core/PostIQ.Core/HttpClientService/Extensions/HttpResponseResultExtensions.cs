using PostIQ.Core.HttpClientService.Models;
using System.Text;
using System.Text.Json;

namespace PostIQ.Core.HttpClientService.Extensions
{
    /// <summary>
    /// Helpers to convert <see cref="HttpResponseResult"/> to typed responses and to read response content.
    /// </summary>
    public static class HttpResponseResultExtensions
    {
        private static readonly JsonSerializerOptions DefaultJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };


        /// <summary>
        /// Convert HttpResponseResult content to a typed object
        /// </summary>
        public static T? ToObject<T>(
            this Models.HttpResponseResult response,
            JsonSerializerOptions? options = null)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (response.Content == null || response.Content.Length == 0)
                return default;

            try
            {
                var json = Encoding.UTF8.GetString(response.Content);
                return JsonSerializer.Deserialize<T>(json, options ?? DefaultJsonOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to deserialize response to {typeof(T).Name}", ex);
            }
        }

        /// <summary>
        /// Async version (supports stream)
        /// </summary>
        public static async Task<T?> ToObjectAsync<T>(
            this Models.HttpResponseResult response,
            JsonSerializerOptions? options = null)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            try
            {
                if (response.ResponseStream != null)
                {
                    return await JsonSerializer.DeserializeAsync<T>(
                        response.ResponseStream,
                        options ?? DefaultJsonOptions);
                }

                if (response.Content != null && response.Content.Length > 0)
                {
                    var json = Encoding.UTF8.GetString(response.Content);
                    return JsonSerializer.Deserialize<T>(json, options ?? DefaultJsonOptions);
                }

                return default;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to deserialize response to {typeof(T).Name}", ex);
            }
        }

        /// <summary>
        /// Get raw string content
        /// </summary>
        public static string? ToRawString(this Models.HttpResponseResult response)
        {
            if (response?.Content == null)
                return null;

            return Encoding.UTF8.GetString(response.Content);
        }

        /// <summary>
        /// Converts the response to <see cref="HttpResponse{T}"/> by deserializing the body as JSON to <typeparamref name="T"/>.
        /// Uses <see cref="HttpResponseResult.Content"/> when present; otherwise reads from <see cref="HttpResponseResult.ResponseStream"/>.
        /// Consumes and disposes the response stream when used.
        /// </summary>
        /// <param name="result">The raw HTTP result.</param>
        /// <param name="jsonOptions">Optional JSON options; default uses camelCase and case-insensitive.</param>
        /// <param name="cancellationToken">Cancellation token when reading from stream.</param>
        /// <returns>Typed response with StatusCode, IsSuccessStatusCode, Headers, Value (deserialized T) and RawBody.</returns>
        public static async Task<HttpResponse<T>> ToResponseAsync<T>(
            this HttpResponseResult result,
            JsonSerializerOptions? jsonOptions = null,
            CancellationToken cancellationToken = default)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            var opts = jsonOptions ?? DefaultJsonOptions;
            string? rawBody = null;
            T? value = default;

            if (result.Content != null && result.Content.Length > 0)
            {
                rawBody = Encoding.UTF8.GetString(result.Content);
                value = JsonSerializer.Deserialize<T>(rawBody, opts);
            }
            else if (result.ResponseStream != null)
            {
                // Read and consume the stream. Dispose after reading.
                using var reader = new StreamReader(result.ResponseStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: false);
                rawBody = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(rawBody))
                {
                    value = JsonSerializer.Deserialize<T>(rawBody, opts);
                }
            }

            return new HttpResponse<T>
            {
                IsSuccessStatusCode = result.IsSuccessStatusCode,
                StatusCode = result.StatusCode,
                ReasonPhrase = result.ReasonPhrase,
                Headers = result.Headers,
                Value = value,
                RawBody = rawBody
            };
        }

        /// <summary>
        /// Converts the response to <see cref="HttpResponse{T}"/> when body is already in memory (Content).
        /// For stream-based results use <see cref="ToResponseAsync{T}"/>.
        /// </summary>
        public static HttpResponse<T> ToResponse<T>(this HttpResponseResult result, JsonSerializerOptions? jsonOptions = null)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            var opts = jsonOptions ?? DefaultJsonOptions;
            string? rawBody = null;
            T? value = default;

            if (result.Content != null && result.Content.Length > 0)
            {
                rawBody = Encoding.UTF8.GetString(result.Content);
                value = JsonSerializer.Deserialize<T>(result.Content, opts);
            }

            return new HttpResponse<T>
            {
                IsSuccessStatusCode = result.IsSuccessStatusCode,
                StatusCode = result.StatusCode,
                ReasonPhrase = result.ReasonPhrase,
                Headers = result.Headers,
                Value = value,
                RawBody = rawBody
            };
        }

        /// <summary>
        /// Gets the response body as a UTF-8 string (from Content or by reading ResponseStream).
        /// For stream results the stream is consumed and disposed.
        /// </summary>
        public static async Task<string> GetContentAsStringAsync(this HttpResponseResult result, CancellationToken cancellationToken = default)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            if (result.Content != null && result.Content.Length > 0)
                return Encoding.UTF8.GetString(result.Content);

            if (result.ResponseStream != null)
            {
                using var reader = new StreamReader(result.ResponseStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: false);
                return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the response body as string when Content is present in memory.
        /// For stream-based results use <see cref="GetContentAsStringAsync"/>.
        /// </summary>
        public static string GetContentAsString(this HttpResponseResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            if (result.Content == null || result.Content.Length == 0)
                return string.Empty;

            return Encoding.UTF8.GetString(result.Content);
        }
    }
}
