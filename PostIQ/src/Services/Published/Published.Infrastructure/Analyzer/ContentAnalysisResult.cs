using PostIQ.Core.AI.Attribute;
using PostIQ.Core.AI.Models;

namespace Published.Infrastructure.Analyzer
{
    /// <summary>
    /// Result of content analysis for social media posting
    /// Extends the generic AnalysisResultBase with specific fields for posting content
    /// </summary>
    public class ContentAnalysisResult : AnalysisResultBase
    {
        public string OriginalTitle { get; set; } = string.Empty;
        public string OriginalAuthor { get; set; } = string.Empty;
        public string OriginalPublishedDate { get; set; } = string.Empty;
        public string OriginalBaseUrl { get; set; } = string.Empty;

        [LlmResponse]
        public string Headline { get; set; } = string.Empty;
        [LlmResponse]
        public string Hook { get; set; } = string.Empty;
        [LlmResponse]
        public string Summary { get; set; } = string.Empty;
        [LlmResponse]
        public string Takeaways { get; set; } = string.Empty;
        [LlmResponse]
        public string CallToAction { get; set; } = string.Empty;
        [LlmResponse]
        public string Hashtags { get; set; } = string.Empty;

        public override Dictionary<string, string> ToDictionary() => new()
    {
        { "original_title", OriginalTitle },
        { "original_author", OriginalAuthor },
        { "original_published_date", OriginalPublishedDate },
        { "original_baseurl", OriginalBaseUrl },
        { "headline", Headline },
        { "summary", Summary },
        { "takeaways", Takeaways },
        { "cta", CallToAction },
        { "hashtags", Hashtags },
        { "analyzed_at", AnalyzedAt }
    };
    }
}
