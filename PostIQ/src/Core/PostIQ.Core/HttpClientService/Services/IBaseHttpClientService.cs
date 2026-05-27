using PostIQ.Core.HttpClientService.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PostIQ.Core.HttpClientService.Models;

namespace PostIQ.Core.HttpClientService.Services
{
    /// <summary>
    /// Abstraction over the generic HTTP client service used across the solution.
    /// Keeps the implementation testable and allows DI of alternative clients.
    /// </summary>
    public interface IBaseHttpClientService
    {
        /// <summary>
        /// Sends an asynchronous HTTP GET request using the specified named client and request URI.
        /// </summary>
        /// <param name="clientName">The logical name of the HTTP client to use for sending the request. This name is typically used to select a
        /// configured client instance. Cannot be null or empty.</param>
        /// <param name="requestUri">The URI of the resource to request. This can be a relative or absolute URI. Cannot be null or empty.</param>
        /// <param name="options">An optional set of request options that configure the request, such as headers or query parameters. If null,
        /// default options are used.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the request operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an HttpResponseResult
        /// representing the HTTP response.</returns>
        Task<HttpResponseResult> GetAsync(string clientName, string requestUri, RequestOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an HTTP GET request to the specified URI using the named client and returns the response as a stream
        /// asynchronously.
        /// </summary>
        /// <remarks>The caller is responsible for disposing the response stream when it is no longer
        /// needed. If the request fails, the returned HttpResponseResult will indicate the error status.</remarks>
        /// <param name="clientName">The logical name of the HTTP client to use for sending the request. This typically corresponds to a
        /// configured client instance.</param>
        /// <param name="requestUri">The URI the request is sent to. This must be a valid absolute or relative URI.</param>
        /// <param name="options">An optional set of request options that configure the request, such as headers or timeout settings. If null,
        /// default options are used.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the request operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an HttpResponseResult with the
        /// response stream and related metadata.</returns>
        Task<HttpResponseResult> GetStreamAsync(string clientName, string requestUri, RequestOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an asynchronous HTTP POST request using the specified client and request URI, with an optional request
        /// body and options.
        /// </summary>
        /// <param name="clientName">The logical name of the HTTP client to use for sending the request. This typically corresponds to a named
        /// client configuration. Cannot be null or empty.</param>
        /// <param name="requestUri">The relative or absolute URI to which the POST request is sent. Cannot be null or empty.</param>
        /// <param name="body">The content to include in the request body. May be null if the request does not require a body.</param>
        /// <param name="options">An optional set of request options that influence the behavior of the HTTP request, such as headers or
        /// timeout settings. May be null to use default options.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the request operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an HttpResponseResult
        /// representing the HTTP response.</returns>
        Task<HttpResponseResult> PostAsync(string clientName, string requestUri, object? body = null, RequestOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an HTTP POST request with form URL-encoded data to the specified URI using the named client.
        /// </summary>
        /// <param name="clientName">The logical name of the HTTP client to use for sending the request. This typically corresponds to a
        /// configured client instance. Cannot be null or empty.</param>
        /// <param name="requestUri">The relative or absolute URI to which the request is sent. Cannot be null or empty.</param>
        /// <param name="formData">A read-only dictionary containing the form fields and values to include in the request body as
        /// application/x-www-form-urlencoded data. Cannot be null.</param>
        /// <param name="options">An optional set of request options that can modify the behavior of the HTTP request, such as custom headers
        /// or timeout settings. May be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the request operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an HttpResponseResult with the
        /// response from the server.</returns>
        Task<HttpResponseResult> PostFormAsync(string clientName, string requestUri, IReadOnlyDictionary<string, string> formData, RequestOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously uploads one or more files to the specified URI using the given HTTP client configuration.
        /// </summary>
        /// <remarks>The method sends the files and any additional form fields as multipart/form-data. The
        /// operation is performed asynchronously and will honor the provided cancellation token. The caller is
        /// responsible for ensuring that the files and form fields conform to the requirements of the target
        /// endpoint.</remarks>
        /// <param name="clientName">The name of the HTTP client to use for the upload operation. This typically corresponds to a configured
        /// client in the application's HTTP client factory.</param>
        /// <param name="requestUri">The URI to which the files will be uploaded. This should be a valid relative or absolute URI accepted by the
        /// target server.</param>
        /// <param name="files">A read-only list of files to upload. Each item represents a file and its associated upload metadata. Cannot
        /// be null or empty.</param>
        /// <param name="additionalFormFields">An optional dictionary of additional form fields to include in the multipart form data. Keys represent field
        /// names; values represent field values. May be null if no extra fields are required.</param>
        /// <param name="options">Optional request options that influence the upload operation, such as custom headers or timeout settings.
        /// May be null to use default options.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the upload operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an HttpResponseResult describing
        /// the outcome of the upload, including status code and any response content from the server.</returns>
        Task<HttpResponseResult> UploadFilesAsync(
            string clientName,
            string requestUri,
            IReadOnlyList<FileUploadItem> files,
            IReadOnlyDictionary<string, string>? additionalFormFields = null,
            RequestOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an asynchronous HTTP PUT request using the specified client, URI, and request body.   
        /// </summary>
        /// <remarks>The method uses the named client to send the request. If the client is not
        /// configured, an exception may be thrown. The request body is serialized according to the configured format.
        /// The operation is performed asynchronously and may be cancelled using the provided token.</remarks>
        /// <param name="clientName">The name of the HTTP client to use for sending the request. Must correspond to a configured client instance.</param>
        /// <param name="requestUri">The URI of the resource to which the PUT request is sent. Must be a valid relative or absolute URI.</param>
        /// <param name="body">The content to include in the request body. Can be null if no body is required.</param>
        /// <param name="options">Optional settings that control request behavior, such as headers or timeout. Can be null to use default
        /// options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. Allows the operation to be cancelled.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response, including
        /// status code and content.</returns>
        Task<HttpResponseResult> PutAsync(string clientName, string requestUri, object? body = null, RequestOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an asynchronous HTTP DELETE request using the specified client and request URI.
        /// </summary>
        /// <param name="clientName">The logical name of the HTTP client to use for sending the request. Cannot be null or empty.</param>
        /// <param name="requestUri">The relative or absolute URI to which the DELETE request is sent. Cannot be null or empty.</param>
        /// <param name="options">An optional set of request options that configure the request, such as headers or query parameters. May be
        /// null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an HttpResponseResult
        /// representing the response from the server.</returns>
        Task<HttpResponseResult> DeleteAsync(string clientName, string requestUri, RequestOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously downloads a file from the specified URI and saves it to the given local file path using the
        /// specified HTTP client configuration.
        /// </summary>
        /// <remarks>If the download fails or is canceled, the returned task will be faulted or canceled,
        /// respectively. The method may overwrite the file at the specified local path if it already exists.</remarks>
        /// <param name="clientName">The name of the HTTP client configuration to use for the download operation. This typically corresponds to a
        /// named client registered in the application's HTTP client factory.</param>
        /// <param name="requestUri">The URI of the file to download. Must be a valid absolute or relative URI supported by the HTTP client.</param>
        /// <param name="localFilePath">The full path, including file name, where the downloaded file will be saved locally. If the file already
        /// exists, it may be overwritten.</param>
        /// <param name="progress">An optional progress reporter that receives the number of bytes downloaded as the operation progresses. Can
        /// be null if progress reporting is not required.</param>
        /// <param name="options">Optional request options that influence the behavior of the download operation, such as custom headers or
        /// timeout settings. Can be null to use default options.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the download operation.</param>
        /// <returns>A task that represents the asynchronous download operation. The task result contains a DownloadResult object
        /// with details about the completed download.</returns>
        Task<DownloadResult> DownloadFileAsync(
            string clientName,
            string requestUri,
            string localFilePath,
            IProgress<long>? progress = null,
            RequestOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously downloads the content from the specified URI and writes it to the provided stream.
        /// </summary>
        /// <remarks>The method does not close or dispose the destination stream. If the download fails or
        /// is canceled, the stream may contain partial data. This method supports reporting download progress and can
        /// be canceled via the provided cancellation token.</remarks>
        /// <param name="clientName">The logical name of the HTTP client to use for the request. This is typically used to select a configured
        /// client instance.</param>
        /// <param name="requestUri">The URI of the resource to download. Must be a valid absolute or relative URI.</param>
        /// <param name="destinationStream">The stream to which the downloaded content will be written. Must be writable and remain open for the
        /// duration of the operation.</param>
        /// <param name="progress">An optional progress reporter that receives the number of bytes downloaded. If specified, progress updates
        /// are reported as the download proceeds.</param>
        /// <param name="options">An optional set of request options that can be used to customize the download operation, such as headers or
        /// timeouts.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the download operation.</param>
        /// <returns>A task that represents the asynchronous download operation. The task completes when the content has been
        /// fully written to the destination stream.</returns>
        Task DownloadStreamAsync(
            string clientName,
            string requestUri,
            Stream destinationStream,
            IProgress<long>? progress = null,
            RequestOptions? options = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously downloads multiple resources to the specified directory using the given client configuration.
        /// </summary>
        /// <remarks>If any download fails, the corresponding entry in the result list will indicate the
        /// failure. The method does not throw for individual download errors; instead, errors are reported in the
        /// result objects. The operation may download resources in parallel, depending on the implementation.</remarks>
        /// <param name="clientName">The name of the client configuration to use for the downloads. Cannot be null or empty.</param>
        /// <param name="requestUris">A list of URIs identifying the resources to download. Cannot be null or empty. Each URI must be a valid,
        /// absolute URI.</param>
        /// <param name="destinationDirectory">The directory path where downloaded files will be saved. Must be a valid, writable directory path.</param>
        /// <param name="progress">An optional progress reporter that receives updates on the number of bytes downloaded for each resource. The
        /// tuple contains the zero-based index of the resource and the number of bytes downloaded so far.</param>
        /// <param name="options">Optional request options that control download behavior, such as headers or timeouts. If null, default
        /// options are used.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the download operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of download
        /// results, one for each requested URI, in the same order as the input list.</returns>
        Task<IReadOnlyList<DownloadResult>> DownloadMultipleAsync(
            string clientName,
            IReadOnlyList<string> requestUris,
            string destinationDirectory,
            IProgress<(int Index, long Bytes)>? progress = null,
            RequestOptions? options = null,
            CancellationToken cancellationToken = default);
    }
}