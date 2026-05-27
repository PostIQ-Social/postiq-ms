using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostIQ.Core.AI.Analyzer;
using PostIQ.Core.AI.Configuration;
using PostIQ.Core.AI.LLM;
using PostIQ.Core.BackgroundProcess.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.AI
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAiServices(this IServiceCollection services, IConfiguration configuration)
        {
                services.Configure<AiConfiguration>(
                    configuration.GetSection(AiConfiguration.SectionName));

            services.AddSingleton<ILlmClient, GeminiLlmClient>();
            return services;
        }
    }
}
