namespace Published.Infrastructure.Providers;

/// <summary>
/// Generic interface for fetching repository/content information from various sources
/// </summary>
public interface IRepositoryProvider
{
    /// <summary>
    /// Fetches repository/content information from a given source
    /// </summary>
    /// <param name="url">The source URL to fetch from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of repository information</returns>
    Task<List<RepositoryInfo>> FetchRepositoriesAsync(string url, CancellationToken cancellationToken = default);
}
