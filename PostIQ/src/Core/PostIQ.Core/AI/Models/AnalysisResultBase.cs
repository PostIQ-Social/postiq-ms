namespace PostIQ.Core.AI.Models;

/// <summary>
/// Base class for LLM analysis results
/// Provides common properties for all content analysis operations
/// </summary>
public abstract class AnalysisResultBase
{
    /// <summary>
    /// Timestamp when the analysis was performed
    /// </summary>
    public string AnalyzedAt { get; set; } = DateTime.UtcNow.ToString("O");

    /// <summary>
    /// The model used for the analysis
    /// </summary>
    public string Model { get; set; } 

    /// <summary>
    /// Converts the analysis result to a dictionary for storage
    /// </summary>
    /// <returns>Dictionary representation of the analysis result</returns>
    public abstract Dictionary<string, string> ToDictionary();
}
