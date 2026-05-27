using Microsoft.Extensions.Options;
using PostIQ.Core.AI.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostIQ.Core.AI.LLM;

/// <summary>
/// Generic Gemini API client for Google's Generative AI
/// Can be used across multiple projects for LLM-based content analysis
/// </summary>
public class GeminiLlmClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private const string ApiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models";

    public GeminiLlmClient(IOptions<AiConfiguration> options)
    {
        _apiKey = options.Value.ApiKey ?? throw new ArgumentNullException(nameof(options.Value.ApiKey));
        _model = options.Value.Model ?? throw new ArgumentNullException(nameof(options.Value.Model));
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Gets a completion from Gemini API
    /// </summary>
    public async Task<string> GetCompletionAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));
        }

        var request = new GeminiRequest
        {
            Contents = new List<GeminiContent>
            {
                new GeminiContent
                {
                    Role = "user",
                    Parts = new List<GeminiPart> { new GeminiPart { Text = prompt } }
                }
            }
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var url = $"{ApiEndpoint}/{_model}:generateContent?key={Uri.EscapeDataString(_apiKey)}";

        try
        {
            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseResponse(responseJson);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to get completion from Gemini API with model {_model}", ex);
        }
    }

    /// <summary>
    /// Gets the name of the model being used
    /// </summary>
    public string GetModelName() => _model;

    /// <summary>
    /// Parses the Gemini API response to extract the text content
    /// </summary>
    private string ParseResponse(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                if (firstCandidate.TryGetProperty("content", out var contentProp) &&
                    contentProp.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0)
                {
                    return parts[0].GetProperty("text").GetString() ?? string.Empty;
                }
            }

            return string.Empty;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse Gemini API response", ex);
        }
    }

    /// <summary>
    /// Gemini API request structure
    /// </summary>
    private class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; set; } = new();

        [JsonPropertyName("generationConfig")]
        public GenerationConfig GenerationConfig { get; set; } = new();
    }

    /// <summary>
    /// Represents a piece of content in the Gemini API request
    /// </summary>
    private class GeminiContent
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; } = new();
    }

    /// <summary>
    /// Represents a part of content (typically text) in the Gemini API request
    /// </summary>
    private class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>
    /// Generation configuration for Gemini API
    /// </summary>
    private class GenerationConfig
    {
        [JsonPropertyName("temperature")]
        public float Temperature { get; set; } = 0.7f;

        [JsonPropertyName("maxOutputTokens")]
        public int MaxOutputTokens { get; set; } = 2048;
    }
}
