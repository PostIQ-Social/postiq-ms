namespace Published.Application.Services
{
    /// <summary>
    /// Service for generating AI-powered catchy social media posts
    /// </summary>
    public interface IPostGenerationService
    {
        /// <summary>
        /// Generates a catchy social media post using AI based on the provided content
        /// </summary>
        /// <param name="headline">The headline or main topic</param>
        /// <param name="originalTitle">The original article/content title</param>
        /// <param name="summary">Brief summary of the content</param>
        /// <param name="takeaways">Key takeaways or main points</param>
        /// <param name="hashtags">Hashtags to include in the post</param>
        /// <param name="cta">Call-to-action text</param>
        /// <param name="originalBaseUrl">URL/link to the original content</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A catchy, AI-generated social media post with all content elements</returns>
        Task<string> GenerateCatchyPostAsync(
            string? headline,
            string? originalTitle,
            string? summary,
            string? takeaways,
            string? hashtags,
            string? cta,
            string? originalBaseUrl,
            CancellationToken cancellationToken = default);
    }
}
