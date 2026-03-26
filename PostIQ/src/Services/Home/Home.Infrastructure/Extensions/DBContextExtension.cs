using Home.Application.Response;
using Home.Core.Entities;
using Home.Core.Persistence;
using Home.Infrastructure.Jobs.PostSyncJob;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostIQ.Core.BackgroundProcess;
using PostIQ.Core.BackgroundProcess.Interfaces;
using PostIQ.Core.Database.Extension;
using PostIQ.Core.HttpClientService.Extensions;

namespace Home.Infrastructure.Extensions
{
    public static class DbContextExtension
    {
        public static IServiceCollection AddDbContextExtension(this IServiceCollection services, IConfiguration configuration)
        {
            // Add HttpClient services
            services.AddHttpClientService(configuration);

            services.AddBackgroundJob(configuration);
            services.AddSingleton<IJobItemProcessor<LastBatchJobResponse>, PostSyncJobProcessor>();
            services.AddSingleton<IJobItemsProducer<LastBatchJobResponse>, PostSyncJobProducer>();
 

            var connectionString = configuration["DefaultConnection"];
            services.AddDbContext<HomeDbContext>(options =>
            {
                options.UseSqlServer(connectionString, o =>
                {
                    o.UseCompatibilityLevel(120);
                });
            }).AddUnitOfWork<HomeDbContext>();
            return services;
        }
    }
}
