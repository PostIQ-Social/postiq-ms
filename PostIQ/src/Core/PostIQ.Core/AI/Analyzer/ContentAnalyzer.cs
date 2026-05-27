using PostIQ.Core.AI.Attribute;
using PostIQ.Core.AI.Helper;
using PostIQ.Core.AI.LLM;

namespace PostIQ.Core.AI.Analyzer
{
    public abstract class ContentAnalyzer<TInput, TOutput> : IContentAnalyzer<TInput, TOutput> 
        where TInput : class
        where TOutput : class
    {
        private readonly ILlmClient _llmClient;

        protected ContentAnalyzer(ILlmClient llmClient = null)
        {
            _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        }

        protected abstract string GetPrompt(TInput input);

        public async Task<TOutput> AnalyzeAsync(TInput input, CancellationToken cancellationToken = default)
        {
            var prompt = GetPrompt(input);
            var response = await _llmClient.GetCompletionAsync(prompt, cancellationToken);

            var llmProperties = typeof(TOutput)
                .GetProperties()
                .Where(p => System.Attribute.IsDefined(p, typeof(LlmResponseAttribute)))
                .Select(p => p.Name)
                .ToArray();

            var output = LlmResponseHelper.GetStringsFromJson(response, llmProperties);
            output.Add("model_used", _llmClient.GetModelName());
            return ParseResponse(input, output);
        }

        protected abstract TOutput ParseResponse(TInput input, Dictionary<string, string> response);
    }
}
