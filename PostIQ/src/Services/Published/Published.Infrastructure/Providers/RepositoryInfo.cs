using PostIQ.Core.AI.Models;

namespace Published.Infrastructure.Providers;

/// <summary>
/// Generic data transfer object for repository/content information
/// Used across different content providers (RSS, APIs, etc.)
/// </summary>
public class RepositoryInfo
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public string Author { get; set; }
    public DateTime? PublishedDate { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}
