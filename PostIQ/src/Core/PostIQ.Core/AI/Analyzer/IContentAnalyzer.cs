namespace PostIQ.Core.AI.Analyzer;

/// <summary>
/// Generic interface for content analysis services
/// Can be implemented by any service that needs to analyze content using LLM
/// </summary>
public interface IContentAnalyzer<TInput, TOutput>
    where TInput : class
    where TOutput : class
{
    /// <summary>
    /// Analyzes input content and returns analysis result
    /// </summary>
    /// <param name="input">The input content to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The analysis result</returns>
    Task<TOutput> AnalyzeAsync(TInput input, CancellationToken cancellationToken = default);
}
