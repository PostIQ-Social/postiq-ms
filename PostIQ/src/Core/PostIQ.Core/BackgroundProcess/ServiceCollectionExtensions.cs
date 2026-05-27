using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostIQ.Core.BackgroundProcess.Configuration;
using PostIQ.Core.BackgroundProcess.Interfaces;
using PostIQ.Core.BackgroundProcess.Services;

namespace PostIQ.Core.BackgroundProcess
{
    /// <summary>
    /// DI extensions for background job infrastructure. Call AddBackgroundJobInfrastructure then register your job hosted services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds background job infrastructure: configuration binding, registry, and trigger service.
        /// Then register your job(s) with AddHostedService<YourJobHostedService> and their IJobItemProducer / IJobItemProcessor.
        /// </summary>
        public static IServiceCollection AddBackgroundJob(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                services.Configure<BackgroundJobsConfiguration>(
                    configuration.GetSection(BackgroundJobsConfiguration.SectionName));
            }
            else
            {
                services.AddOptions<BackgroundJobsConfiguration>();
            }

            services.AddSingleton<IBackgroundJobRegistry, BackgroundJobRegistry>();
            services.AddSingleton<IBackgroundJobTrigger, BackgroundJobTriggerService>();

            return services;
        }
    }
}
