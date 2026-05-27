using PostIQ.Core.AI.LLM;
using System.Text;

namespace Published.Application.Services
{
    /// <summary>
    /// AI-powered service for generating catchy social media posts using Gemini LLM
    /// </summary>
    public class PostGenerationService : IPostGenerationService
    {
        private readonly ILlmClient _llmClient;

        public PostGenerationService(ILlmClient llmClient)
        {
            _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        }

        /// <summary>
        /// Generates a catchy social media post using AI based on the provided content
        /// </summary>
        public async Task<string> GenerateCatchyPostAsync(
            string? headline,
            string? originalTitle,
            string? summary,
            string? takeaways,
            string? hashtags,
            string? cta,
            string? originalBaseUrl,
            CancellationToken cancellationToken = default)
        {
            // Build a comprehensive context for the AI model
            var contextBuilder = new StringBuilder();

            contextBuilder.AppendLine("Create a catchy, engaging social media post based on the following content:");
            contextBuilder.AppendLine();

            if (!string.IsNullOrWhiteSpace(headline))
            {
                contextBuilder.AppendLine($"Headline: {headline}");
            }

            if (!string.IsNullOrWhiteSpace(originalTitle))
            {
                contextBuilder.AppendLine($"Title: {originalTitle}");
            }

            if (!string.IsNullOrWhiteSpace(summary))
            {
                contextBuilder.AppendLine($"Summary: {summary}");
            }

            if (!string.IsNullOrWhiteSpace(takeaways))
            {
                contextBuilder.AppendLine($"Key Points: {takeaways}");
            }

            if (!string.IsNullOrWhiteSpace(cta))
            {
                contextBuilder.AppendLine($"Call-to-Action: {cta}");
            }

            if (!string.IsNullOrWhiteSpace(originalBaseUrl))
            {
                contextBuilder.AppendLine($"Source URL: {originalBaseUrl}");
            }

            if (!string.IsNullOrWhiteSpace(hashtags))
            {
                contextBuilder.AppendLine($"Hashtags to Include: {hashtags}");
            }

            contextBuilder.AppendLine();
            contextBuilder.AppendLine("Requirements for the post:");
            contextBuilder.AppendLine("- The response MUST be formatted in GitHub-flavored Markdown");
            contextBuilder.AppendLine("- Make it catchy and attention-grabbing");
            contextBuilder.AppendLine("- Keep it concise (2-3 paragraphs max)");
            contextBuilder.AppendLine("- Use engaging language and relevant emojis");
            contextBuilder.AppendLine("- Use **bold**, _italics_, headings, lists, and links where appropriate");
            contextBuilder.AppendLine("- Do not use <br> tags or raw HTML.");
            contextBuilder.AppendLine("- Use blank lines (`\\n\\n`) for paragraph breaks.\r\n");
            contextBuilder.AppendLine("- Incorporate the provided call-to-action naturally");
            contextBuilder.AppendLine("- Format all links as Markdown hyperlinks with descriptive titles.");
            contextBuilder.AppendLine("- Do not show raw URLs. Instead, use the article title or a short descriptive phrase as the link text.");
            contextBuilder.AppendLine("- Example: **Read more:** [Let’s Build a gRPC Microservice in .NET (Step by Step)](https://medium.com/...)");
            contextBuilder.AppendLine("- Add the provided hashtags at the very end");
            contextBuilder.AppendLine("- Format hashtags as Markdown links that point to a hashtag search page.");
            contextBuilder.AppendLine("- Example: [#DotNet](/dotnet)");
            contextBuilder.AppendLine("- Do not output plain #hashtags; always make them clickable links.");
            contextBuilder.AppendLine("- Make it suitable for social media (LinkedIn, Twitter, etc.)");
            contextBuilder.AppendLine("- Maintain a professional yet conversational tone");
            contextBuilder.AppendLine("- Ensure the post is self-contained and complete");

            var prompt = contextBuilder.ToString();

            try
            {
                var aiGeneratedPost = await _llmClient.GetCompletionAsync(prompt, cancellationToken);
                return aiGeneratedPost;
            }
            catch (Exception ex)
            {
                // Log the error and return empty string if AI generation fails
                // This ensures graceful degradation
                System.Diagnostics.Debug.WriteLine($"AI post generation failed: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
