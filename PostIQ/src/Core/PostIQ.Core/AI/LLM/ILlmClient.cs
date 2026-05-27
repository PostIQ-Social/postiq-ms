namespace PostIQ.Core.AI.LLM
{
    /// <summary>
    /// Generic interface for LLM (Large Language Model) clients
    /// Supports any LLM provider (OpenAI, Gemini, Claude, etc.)
    /// </summary>
    public interface ILlmClient
    {
        /// <summary>
        /// Gets a completion from the LLM with the given prompt
        /// </summary>
        /// <param name="prompt">The prompt to send to the LLM</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The LLM response text</returns>
        Task<string> GetCompletionAsync(string prompt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the name/identifier of the LLM model being used
        /// </summary>
        string GetModelName();
    }
}
