using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;

namespace Published.Infrastructure.Providers;

/// <summary>
/// Utility class for parsing RSS feeds and extracting content information
/// </summary>
public class RssFeedParser
{
    /// <summary>
    /// Parses RSS feed content and extracts repository information
    /// </summary>
    /// <param name="feedContent">The RSS feed XML content</param>
    /// <param name="sourceUrl">The original source URL</param>
    /// <returns>List of RepositoryInfo extracted from the feed</returns>
    public static List<RepositoryInfo> ParseRssFeed(string feedContent, string sourceUrl)
    {
        var repositories = new List<RepositoryInfo>();

        try
        {
            using (var reader = XmlReader.Create(new StringReader(feedContent)))
            {
                var feed = SyndicationFeed.Load(reader);

                // Extract feed author/publisher
                string? feedAuthor = feed.Title?.Text ?? ExtractAuthorFromUrl(sourceUrl);

                foreach (var item in feed.Items)
                {
                    var repositoryInfo = ExtractRepositoryFromFeedItem(item, feedAuthor, sourceUrl);
                    repositories.Add(repositoryInfo);
                }
            }
        }
        catch (XmlException xmlEx)
        {
            throw new InvalidOperationException("Invalid RSS feed format", xmlEx);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse RSS feed", ex);
        }

        return repositories;
    }

    /// <summary>
    /// Extracts repository information from a single RSS feed item
    /// </summary>
    private static RepositoryInfo ExtractRepositoryFromFeedItem(
        SyndicationItem item,
        string? feedAuthor,
        string sourceUrl)
    {
        var publishedDate = item.PublishDate != default(DateTimeOffset)
            ? item.PublishDate.DateTime
            : DateTime.UtcNow;

        var metadata = new Dictionary<string, string>
        {
            { "source", "rss" },
            { "fetchedAt", DateTime.UtcNow.ToString("O") }
        };

        // Extract categories/tags
        if (item.Categories.Count > 0)
        {
            var tags = string.Join(", ", item.Categories.Select(c => c.Name));
            metadata.Add("tags", tags);
        }

        // Extract content/summary
        string? description = null;
        if (item.Summary != null)
        {
            description = StripHtmlTags(item.Summary.Text);
        }

        var contentElement = item.ElementExtensions
        .FirstOrDefault(ext => ext.OuterName == "encoded" && ext.OuterNamespace == "http://purl.org/rss/1.0/modules/content/");

        if (contentElement != null)
        {
            var reader = contentElement.GetReader();
            var rawContent = reader.ReadInnerXml();
            description = StripHtmlTags(rawContent);
        }


        // Extract authors
        string? author = null;
        if (item.Authors.Count > 0)
        {
            author = item.Authors.First().Name ?? item.Authors.First().Email;
        }
        else if (!string.IsNullOrEmpty(feedAuthor))
        {
            author = feedAuthor;
        }

        // Extract links - get the first link as the article URL
        var articleUrl = item.Links.FirstOrDefault()?.Uri.ToString() ?? sourceUrl;

        // Extract update date if available
        if (item.LastUpdatedTime != default(DateTimeOffset))
        {
            metadata.Add("updatedAt", item.LastUpdatedTime.ToString("O"));
        }

        return new RepositoryInfo
        {
            Title = item.Title?.Text,
            Description = description,
            Url = articleUrl,
            Author = author,
            PublishedDate = publishedDate,
            Metadata = metadata
        };
    }

    /// <summary>
    /// Extracts author name from URL
    /// </summary>
    /// <param name="url">The URL</param>
    /// <returns>Extracted author name or null</returns>
    private static string? ExtractAuthorFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath.TrimStart('/');

            // Handle URLs with @ prefix like Medium (@username)
            if (path.StartsWith("@"))
            {
                return path[1..]; // Remove the @ symbol
            }

            // Handle other formats
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0 && parts[0].StartsWith("@"))
            {
                return parts[0][1..];
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Removes HTML tags from text
    /// </summary>
    private static string StripHtmlTags(string html)
    {
        if (string.IsNullOrEmpty(html))
            return html;

        // Remove HTML tags
        var cleanText = Regex.Replace(html, "<[^>]+>", " ");

        // Decode HTML entities
        cleanText = System.Net.WebUtility.HtmlDecode(cleanText);

        // Remove extra whitespace
        cleanText = Regex.Replace(cleanText, @"\s+", " ").Trim();

        return cleanText;
    }
}
