using PostIQ.Core.AI.Analyzer;
using PostIQ.Core.AI.LLM;
using Published.Infrastructure.Providers;

namespace Published.Infrastructure.Analyzer
{
    public class MediumContentAnalyzer : ContentAnalyzer<RepositoryInfo, ContentAnalysisResult>
    {
        public MediumContentAnalyzer(ILlmClient llmClient) : base(llmClient)
        {
        }

        protected override string GetPrompt(RepositoryInfo input)
        {
            return $@"Transform this blog post into a LinkedIn power post that drives engagement!

* ARTICLE DETAILS:
Title: {input.Title}
Author: {input.Author}
Description: {input.Description}

* YOUR MISSION:
Create a compelling LinkedIn post that captures the essence of this article and makes professionals STOP scrolling and START engaging.

* REQUIRED JSON OUTPUT (return ONLY valid JSON, no markdown):
{{
  ""headline"": ""A punchy, curiosity-driven headline (max 100 chars) with relevant emojis - NO quotes"",
  ""hook"": ""An irresistible opening line (max 100 chars) that makes people want to read more"",
  ""summary"": ""A compelling 280-char summary that highlights the value proposition and why it matters"",
  ""takeaways"": ""3 key takeaways or insights from the article, each on a new line, formatted as:\n✓ Point 1\n✓ Point 2\n✓ Point 3"",
  ""calltoaction"": ""A clear call-to-action that encourages engagement - ask a question, invite sharing, or suggest reading the full article"",
  ""hashtags"": ""7-10 trending, relevant hashtags for discoverability - mix of popular (#Leadership, #Innovation) and niche tags""
}}

* TONE REQUIREMENTS:
- Professional yet conversational
- Action-oriented and inspiring
- Include relevant emojis in headline
- Make it shareable and discussion-worthy
- Focus on value, not self-promotion";
        }

        protected override ContentAnalysisResult ParseResponse(RepositoryInfo input, Dictionary<string, string> response)
        {
            return new ContentAnalysisResult
            {
                OriginalTitle = input.Title ?? string.Empty,
                OriginalAuthor = input.Author ?? string.Empty,
                OriginalPublishedDate = input.PublishedDate?.ToString() ?? string.Empty,
                OriginalBaseUrl = input.Url ?? string.Empty,
                Headline = response["headline"],
                Summary = response["summary"],
                Hook = response["hook"],
                Takeaways = response["takeaways"],
                CallToAction = response["calltoaction"],
                Hashtags = response["hashtags"],
                AnalyzedAt = DateTime.UtcNow.ToString("O"),
                Model = response["model_used"]
            };
        }
    }
}
