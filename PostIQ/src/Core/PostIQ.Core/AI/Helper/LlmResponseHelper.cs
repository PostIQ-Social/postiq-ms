using System.Text.Json;

namespace PostIQ.Core.AI.Helper;

/// <summary>
/// Generic utility class for working with LLM responses
/// Provides common operations for parsing and processing LLM outputs
/// </summary>
public static class LlmResponseHelper
{
    /// <summary>
    /// Cleans JSON strings from LLM responses that may contain markdown formatting
    /// </summary>
    /// <param name="response">The raw LLM response potentially containing markdown</param>
    /// <returns>Cleaned JSON string</returns>
    public static string CleanJsonResponse(string response)
    {
        if (string.IsNullOrEmpty(response))
            return response;

        var cleaned = response.Trim();
        
        // Remove markdown code block formatting
        if (cleaned.StartsWith("```json"))
            cleaned = cleaned[7..];
        else if (cleaned.StartsWith("```"))
            cleaned = cleaned[3..];
        
        if (cleaned.EndsWith("```"))
            cleaned = cleaned[..^3];
        
        return cleaned.Trim();
    }

    /// <summary>
    /// Parses a JSON response from an LLM and extracts a string value by property name
    /// </summary>
    /// <param name="jsonResponse">The JSON response string</param>
    /// <param name="propertyName">The name of the property to extract</param>
    /// <returns>The string value of the property, or empty string if not found</returns>
    public static string GetStringFromJson(string jsonResponse, string propertyName)
    {
        try
        {
            var cleaned = CleanJsonResponse(jsonResponse);
            using var doc = JsonDocument.Parse(cleaned);
            return GetString(doc.RootElement, propertyName);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Parses a JSON response from an LLM into a dictionary of string values
    /// Useful for extracting multiple fields at once
    /// </summary>
    /// <param name="jsonResponse">The JSON response string</param>
    /// <param name="propertyNames">The names of properties to extract</param>
    /// <returns>Dictionary with property names as keys and their values</returns>
    public static Dictionary<string, string> GetStringsFromJson(string jsonResponse, params string[] propertyNames)
    {
        var result = new Dictionary<string, string>();
        
        try
        {
            var cleaned = CleanJsonResponse(jsonResponse);
            using var doc = JsonDocument.Parse(cleaned);
            var root = doc.RootElement;

            foreach (var propertyName in propertyNames)
            {
                result[propertyName.ToLower()] = GetString(root, propertyName.ToLower());
            }
        }
        catch
        {
            // If parsing fails, return empty values for all requested properties
            foreach (var propertyName in propertyNames)
            {
                result[propertyName] = string.Empty;
            }
        }

        return result;
    }

    /// <summary>
    /// Gets a string value from a JsonElement by property name
    /// </summary>
    private static string GetString(JsonElement element, string property)
    {
        return element.TryGetProperty(property, out var prop) && prop.ValueKind == JsonValueKind.String
            ? prop.GetString() ?? string.Empty
            : string.Empty;
    }

    /// <summary>
    /// Validates that a JSON string is valid
    /// </summary>
    public static bool IsValidJson(string jsonString)
    {
        try
        {
            var cleaned = CleanJsonResponse(jsonString);
            using var _ = JsonDocument.Parse(cleaned);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
