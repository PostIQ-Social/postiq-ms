using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PostIQ.Core.HttpClientService.Models;
using PostIQ.Core.HttpClientService.Services;

namespace Published.Infrastructure.Providers;

public class MediumRepositoryProvider : IRepositoryProvider
{
    private readonly IBaseHttpClientService _httpClientService;

    public MediumRepositoryProvider(IBaseHttpClientService httpClientService)
    {
        _httpClientService = httpClientService;
    }

    public async Task<List<RepositoryInfo>> FetchRepositoriesAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty", nameof(url));
        }

        try
        {
            // Convert Medium profile URL to RSS feed URL
            var feedUrl = ConvertToMediumRssFeedUrl(url);

            // Use BaseHttpClientService to get a stream for the RSS feed to avoid buffering large responses.
            // Use empty client name to use the default IHttpClientFactory client; change if a named client is desired.
            HttpResponseResult result = await _httpClientService.GetStreamAsync(string.Empty, feedUrl, options: null, cancellationToken);

            if (!result.IsSuccessStatusCode || result.ResponseStream is null)
            {
                throw new InvalidOperationException($"Failed to fetch RSS feed from Medium URL: {url}. StatusCode={result.StatusCode} Reason={result.ReasonPhrase}");
            }

            using (result)
            {
                using var reader = new StreamReader(result.ResponseStream, Encoding.UTF8);
                var feedContent = await reader.ReadToEndAsync();

                // Parse RSS feed and extract repository information
                var repositories = RssFeedParser.ParseRssFeed(feedContent, url);

                return repositories;
            }
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Failed to fetch RSS feed from Medium URL: {url}. The profile might not exist or RSS feed is not accessible.",
                ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to process Medium RSS feed for URL: {url}",
                ex);
        }
    }

    /// <summary>
    /// Converts a Medium profile URL to its RSS feed URL
    /// Examples:
    /// - https://medium.com/@username -> https://medium.com/feed/@username
    /// - https://medium.com/@username/ -> https://medium.com/feed/@username
    /// - https://medium.com/feed/@username -> https://medium.com/feed/@username (already valid)
    /// </summary>
    private string ConvertToMediumRssFeedUrl(string profileUrl)
    {
        try
        {
            var uri = new Uri(profileUrl);

            // Check if it's already a feed URL
            if (uri.AbsolutePath.StartsWith("/feed/"))
            {
                return profileUrl;
            }

            // Extract the path and validate it contains @username
            var path = uri.AbsolutePath.TrimEnd('/');

            if (!path.Contains("@"))
            {
                throw new ArgumentException(
                    "Invalid Medium URL format. URL should contain a username (e.g., @username)",
                    nameof(profileUrl));
            }

            // Reconstruct the feed URL
            var feedUrl = $"{uri.Scheme}://{uri.Host}/feed{path}";
            return feedUrl;
        }
        catch (UriFormatException)
        {
            throw new ArgumentException("Invalid URL format", nameof(profileUrl));
        }
    }
}
