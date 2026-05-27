namespace Published.Infrastructure.Providers;

public class MediumRepositoryProvider : IRepositoryProvider
{
    private readonly HttpClient _httpClient;

    public MediumRepositoryProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
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

            // Fetch the RSS feed
            var response = await _httpClient.GetAsync(feedUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var feedContent = await response.Content.ReadAsStringAsync(cancellationToken);

            // Parse RSS feed and extract repository information
            var repositories = RssFeedParser.ParseRssFeed(feedContent, url);

            return repositories;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (HttpRequestException httpEx)
        {
            throw new InvalidOperationException(
                $"Failed to fetch RSS feed from Medium URL: {url}. The profile might not exist or RSS feed is not accessible.", 
                httpEx);
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
