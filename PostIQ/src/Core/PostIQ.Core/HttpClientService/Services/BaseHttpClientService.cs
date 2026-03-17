using Microsoft.Extensions.Options;
using PostIQ.Core.HttpClientService.Configuration;
using PostIQ.Core.HttpClientService.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PostIQ.Core.HttpClientService.Services
{
    /// <summary>
    /// High-performance generic HTTP client supporting all methods, single/multiple file upload and download,
    /// and large files via streaming and configurable timeouts.
    /// - Uses IHttpClientFactory (do NOT dispose returned HttpClient).
    /// - Streams large responses to avoid buffering when requested.
    /// - Honors per-client and per-request timeouts.
    /// </summary>
    public sealed class BaseHttpClientService : IBaseHttpClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClientSettings _settings;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public BaseHttpClientService(IHttpClientFactory httpClientFactory, IOptions<HttpClientSettings> settings)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _settings = settings?.Value ?? new HttpClientSettings();
        }

        private System.Net.Http.HttpClient CreateClient(string clientName) // Fully qualify HttpClient
        {
            var client = _httpClientFactory.CreateClient(clientName);

            var opts = GetOptions(clientName);
            if (opts != null)
            {
                if (opts.Timeout.HasValue)
                    client.Timeout = opts.Timeout.Value;

                // when registering named clients
                if (!string.IsNullOrWhiteSpace(opts.BaseAddress))
                    client.BaseAddress = new Uri(opts.BaseAddress.TrimEnd('/') + "/", UriKind.RelativeOrAbsolute);

                // Apply default headers on the HttpClient level (fast path).
                client.DefaultRequestHeaders.Clear();
                foreach (var h in opts.DefaultHeaders ?? Enumerable.Empty<KeyValuePair<string, string>>())
                {
                    // TryAddWithoutValidation because some headers (like User-Agent) require special handling.
                    client.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, h.Value);
                }

                // If AutomaticDecompression is requested, add Accept-Encoding header so servers
                // may send compressed content. NOTE: actual automatic decompression is performed
                // by the HttpMessageHandler (HttpClientHandler.AutomaticDecompression). To get
                // automatic decompression, configure the primary handler when registering the
                // named client with IHttpClientFactory (recommended). Adding Accept-Encoding
                // here is a safe fallback when handler is configured elsewhere.
                if (opts.AutomaticDecompression)
                {
                    // Only add if not already present
                    if (!client.DefaultRequestHeaders.AcceptEncoding.Any())
                    {
                        client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("br"));
                        client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                        client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
                    }
                }

            }

            return client;
        }

        private HttpClientOptions? GetOptions(string clientName)
            => _settings.Clients != null && _settings.Clients.TryGetValue(clientName, out var opts) ? opts : null;

        private static void ApplyRequestOptions(HttpRequestMessage request, RequestOptions? options, HttpClientOptions? clientOpts)
        {
            if (options == null && clientOpts == null) return;

            // Per-request headers (override client defaults)
            if (options?.Headers != null)
            {
                foreach (var h in options.Headers)
                {
                    if (!request.Headers.TryAddWithoutValidation(h.Key, h.Value))
                        request.Content?.Headers.TryAddWithoutValidation(h.Key, h.Value);
                }
            }

            // Authorization: Bearer token takes precedence over BasicAuth
            var token = options?.BearerToken;
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else if (options?.BasicAuth.HasValue == true)
            {
                var (username, password) = options.BasicAuth.Value;
                var bytes = Encoding.UTF8.GetBytes($"{username}:{password}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));
            }

            // Client-level default headers that are not already present are already applied on HttpClient in CreateClient.
        }

        private async Task<HttpResponseResult> SendAsync(
            string clientName,
            HttpRequestMessage request,
            RequestOptions? options,
            bool readAsStream,
            CancellationToken cancellationToken)
        {
            options ??= new RequestOptions();
            var opts = GetOptions(clientName);
            var client = CreateClient(clientName);

            // Determine completion option
            var completionOption = options.CompletionOption;
            // If caller requested streaming and didn't explicitly set ResponseContentRead, prefer ResponseHeadersRead
            if (readAsStream && completionOption == HttpCompletionOption.ResponseContentRead)
                completionOption = HttpCompletionOption.ResponseHeadersRead;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            // Per-request timeout overrides per-client timeout if set
            if (options.Timeout.HasValue)
                cts.CancelAfter(options.Timeout.Value);
            else if (opts?.OverallTimeout.HasValue == true)
                cts.CancelAfter(opts.OverallTimeout.Value);

            ApplyRequestOptions(request, options, opts);

            HttpResponseMessage response = null!;
            try
            {
                response = await client.SendAsync(request, completionOption, cts.Token).ConfigureAwait(false);

                // Aggregate headers (response + content headers)
                var headers = response.Headers.Concat(response.Content?.Headers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
                    .ToDictionary(h => h.Key, h => string.Join(" ", h.Value), StringComparer.OrdinalIgnoreCase);

                if (readAsStream)
                {
                    // Return stream; caller must dispose HttpResponseResult which disposes stream
                    var stream = await response.Content.ReadAsStreamAsync(cts.Token).ConfigureAwait(false);
                    return new HttpResponseResult
                    {
                        IsSuccessStatusCode = response.IsSuccessStatusCode,
                        StatusCode = (int)response.StatusCode,
                        ReasonPhrase = response.ReasonPhrase,
                        Headers = headers,
                        ResponseStream = stream
                    };
                }
                else
                {
                    // Respect MaxResponseContentBufferSize: if zero or null indicates streaming only - but caller requested non-stream,
                    // so we will still buffer (caller asked), but guard memory usage.
                    byte[]? content = null;
                    if (response.Content != null)
                    {
                        content = await response.Content.ReadAsByteArrayAsync(cts.Token).ConfigureAwait(false);
                    }

                    return new HttpResponseResult
                    {
                        IsSuccessStatusCode = response.IsSuccessStatusCode,
                        StatusCode = (int)response.StatusCode,
                        ReasonPhrase = response.ReasonPhrase,
                        Headers = headers,
                        Content = content ?? Array.Empty<byte>()
                    };
                }
            }
            finally
            {
                // If we returned a stream we intentionally did not dispose response.Content/response here since the stream still references it.
                // In the non-stream case, dispose the response to free resources.
                if (!readAsStream)
                {
                    response?.Dispose();
                    request?.Dispose();
                }
                else
                {
                    // dispose the request early; response/stream will be disposed by HttpResponseResult.Dispose when caller disposes.
                    request?.Dispose();
                }
            }
        }

        public Task<HttpResponseResult> GetAsync(string clientName, string requestUri, RequestOptions? options = null, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            return SendAsync(clientName, request, options, readAsStream: false, cancellationToken);
        }

        public Task<HttpResponseResult> GetStreamAsync(string clientName, string requestUri, RequestOptions? options = null, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            return SendAsync(clientName, request, options, readAsStream: true, cancellationToken);
        }

        public async Task<HttpResponseResult> PostAsync(string clientName, string requestUri, object? body = null, RequestOptions? options = null, CancellationToken cancellationToken = default)
        {
            HttpContent? content = null;
            if (body != null)
            {
                if (body is HttpContent existingContent)
                    content = existingContent;
                else
                    content = JsonContent.Create(body, options: JsonOptions);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = content };
            return await SendAsync(clientName, request, options, readAsStream: false, cancellationToken).ConfigureAwait(false);
        }

        public Task<HttpResponseResult> PostFormAsync(string clientName, string requestUri, IReadOnlyDictionary<string, string> formData, RequestOptions? options = null, CancellationToken cancellationToken = default)
        {
            var form = new FormUrlEncodedContent(formData ?? new Dictionary<string, string>());
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = form };
            return SendAsync(clientName, request, options, readAsStream: false, cancellationToken);
        }

        public async Task<HttpResponseResult> UploadFilesAsync(
            string clientName,
            string requestUri,
            IReadOnlyList<FileUploadItem> files,
            IReadOnlyDictionary<string, string>? additionalFormFields = null,
            RequestOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (files == null || files.Count == 0)
                throw new ArgumentException("At least one file must be provided", nameof(files));

            var opts = GetOptions(clientName);
            var bufferSize = opts?.BufferSize ?? 81920;

            using var multipart = new MultipartFormDataContent();

            foreach (var kv in additionalFormFields ?? new Dictionary<string, string>())
            {
                multipart.Add(new StringContent(kv.Value), kv.Key);
            }

            foreach (var file in files)
            {
                if (file == null) continue;

                var content = new StreamContent(file.Content, bufferSize);
                content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");

                try
                {
                    if (file.Content.CanSeek)
                        content.Headers.ContentLength = file.Content.Length;
                }
                catch
                {
                    // ignore if stream doesn't support length
                }

                multipart.Add(content, file.FormFieldName, file.FileName);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = multipart };
            return await SendAsync(clientName, request, options, readAsStream: false, cancellationToken).ConfigureAwait(false);
        }

        public async Task<HttpResponseResult> PutAsync(string clientName, string requestUri, object? body = null, RequestOptions? options = null, CancellationToken cancellationToken = default)
        {
            HttpContent? content = null;
            if (body != null)
            {
                if (body is HttpContent c)
                    content = c;
                else
                    content = JsonContent.Create(body, options: JsonOptions);
            }

            var request = new HttpRequestMessage(HttpMethod.Put, requestUri) { Content = content };
            return await SendAsync(clientName, request, options, readAsStream: false, cancellationToken).ConfigureAwait(false);
        }

        public Task<HttpResponseResult> DeleteAsync(string clientName, string requestUri, RequestOptions? options = null, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            return SendAsync(clientName, request, options, readAsStream: false, cancellationToken);
        }

        public async Task<DownloadResult> DownloadFileAsync(
            string clientName,
            string requestUri,
            string localFilePath,
            IProgress<long>? progress = null,
            RequestOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var opts = GetOptions(clientName);
            var bufferSize = opts?.BufferSize ?? 81920;
            options ??= new RequestOptions();
            options.CompletionOption = HttpCompletionOption.ResponseHeadersRead;

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var client = CreateClient(clientName);
            ApplyRequestOptions(request, options, opts);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (options.Timeout.HasValue)
                cts.CancelAfter(options.Timeout.Value);
            else if (opts?.OverallTimeout.HasValue == true)
                cts.CancelAfter(opts.OverallTimeout.Value);

            HttpResponseMessage? response = null;
            try
            {
                response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token).ConfigureAwait(false);

                var result = new DownloadResult
                {
                    Success = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    FileNameFromHeader = response.Content?.Headers?.ContentDisposition?.FileName?.Trim('\"')
                };

                if (!response.IsSuccessStatusCode)
                {
                    return result;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(localFilePath) ?? ".");

                await using var responseStream = await response.Content.ReadAsStreamAsync(cts.Token).ConfigureAwait(false);
                await using var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);

                var buffer = new byte[bufferSize];
                long total = 0;
                int read;
                while ((read = await responseStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cts.Token).ConfigureAwait(false)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, read), cts.Token).ConfigureAwait(false);
                    total += read;
                    progress?.Report(total);
                }

                result.Success = true;
                result.BytesWritten = total;
                result.LocalFilePath = localFilePath;

                return result;
            }
            finally
            {
                response?.Dispose();
                request.Dispose();
            }
        }

        public async Task DownloadStreamAsync(
            string clientName,
            string requestUri,
            Stream destinationStream,
            IProgress<long>? progress = null,
            RequestOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (destinationStream == null) throw new ArgumentNullException(nameof(destinationStream));

            var opts = GetOptions(clientName);
            var bufferSize = opts?.BufferSize ?? 81920;
            options ??= new RequestOptions();
            options.CompletionOption = HttpCompletionOption.ResponseHeadersRead;

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var client = CreateClient(clientName);
            ApplyRequestOptions(request, options, opts);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (options.Timeout.HasValue)
                cts.CancelAfter(options.Timeout.Value);
            else if (opts?.OverallTimeout.HasValue == true)
                cts.CancelAfter(opts.OverallTimeout.Value);

            HttpResponseMessage? response = null;
            try
            {
                response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                await using var responseStream = await response.Content.ReadAsStreamAsync(cts.Token).ConfigureAwait(false);

                var buffer = new byte[bufferSize];
                long total = 0;
                int read;
                while ((read = await responseStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cts.Token).ConfigureAwait(false)) > 0)
                {
                    await destinationStream.WriteAsync(buffer.AsMemory(0, read), cts.Token).ConfigureAwait(false);
                    total += read;
                    progress?.Report(total);
                }
            }
            finally
            {
                response?.Dispose();
                request.Dispose();
            }
        }

        public async Task<IReadOnlyList<DownloadResult>> DownloadMultipleAsync(
            string clientName,
            IReadOnlyList<string> requestUris,
            string destinationDirectory,
            IProgress<(int Index, long Bytes)>? progress = null,
            RequestOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (requestUris == null) throw new ArgumentNullException(nameof(requestUris));
            Directory.CreateDirectory(destinationDirectory);

            var results = new List<DownloadResult>(requestUris.Count);
            for (var i = 0; i < requestUris.Count; i++)
            {
                var uri = requestUris[i];
                var fileName = Path.GetFileName(uri) ?? $"download_{i}";
                var localPath = Path.Combine(destinationDirectory, fileName);

                var perFileProgress = progress is null ? null : new Progress<long>(bytes => progress.Report((i, bytes)));
                var result = await DownloadFileAsync(clientName, uri, localPath, perFileProgress, options, cancellationToken).ConfigureAwait(false);

                // If Content-Disposition has a filename prefer that and move file
                if (result.Success && !string.IsNullOrEmpty(result.FileNameFromHeader))
                {
                    var betterPath = Path.Combine(destinationDirectory, result.FileNameFromHeader);
                    if (!string.Equals(localPath, betterPath, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            if (File.Exists(localPath))
                                File.Move(localPath, betterPath, overwrite: true);

                            result.LocalFilePath = betterPath;
                        }
                        catch
                        {
                            // ignore rename errors, keep original path
                        }
                    }
                }

                results.Add(result);
            }

            return results;
        }
    }
}
