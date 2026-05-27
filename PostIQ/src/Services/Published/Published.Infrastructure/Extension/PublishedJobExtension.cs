using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostIQ.Core.AI;
using PostIQ.Core.AI.Analyzer;
using PostIQ.Core.BackgroundProcess;
using PostIQ.Core.BackgroundProcess.Interfaces;
using Published.Core.Entities;
using Published.Infrastructure.Analyzer;
using Published.Infrastructure.Jobs.RepoDetailsJobs;
using Published.Infrastructure.Jobs.RepoJobs;
using Published.Infrastructure.Providers;

namespace Published.Infrastructure.Extension
{
    public static class PublishedJobExtension
    {
        public static IServiceCollection AddJobExtension(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddBackgroundJob(configuration);
            services.AddHttpClient();
            services.AddAiServices(configuration);
            services.AddScoped<ContentAnalyzer<RepositoryInfo, ContentAnalysisResult>, MediumContentAnalyzer>();
            services.AddScoped<MediumRepositoryProvider>();
            services.AddScoped<IRepositoryProviderFactory, RepositoryProviderFactory>();
            services.AddScoped<IRepositoryProvider, MediumRepositoryProvider>();

            services.AddSingleton<IJobItemProcessor<Job>, RepoJobProcessor>();
            services.AddSingleton<IJobItemsProducer<Job>, RepoJobProducer>();
            services.AddSingleton<IJobItemProcessor<Repo>, RepoDetailsJobProcessor>();
            services.AddSingleton<IJobItemsProducer<Repo>, RepoDetailsJobProducer>();
            return services;
        }
    }
}
