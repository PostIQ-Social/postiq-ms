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
using PostIQ.Core.Shared.Extensions;

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

            services.AddDbContext<HomeDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("Default"), o =>
                {
                    o.UseCompatibilityLevel(120);
                });
            }).AddUnitOfWork<HomeDbContext>();

            services.AddServiceCollectionExtensions(configuration,
                typeof(Home.Core.Entities.Post).Assembly,
                typeof(Home.Application.Handlers.GetLastJobHandler).Assembly,
                typeof(Home.Infrastructure.Extensions.DbContextExtension).Assembly);

            return services;
        }
    }
}
