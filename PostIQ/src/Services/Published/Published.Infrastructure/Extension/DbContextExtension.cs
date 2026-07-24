using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostIQ.Core.Database.Extension;
using PostIQ.Core.Shared.Extensions;
using Published.Core.Persistence;

namespace Published.Infrastructure.Extension
{
    public static class DbContextExtension
    {
        public static IServiceCollection AddDbContextExtension(this IServiceCollection services, IConfiguration configuration)
        { 
            services.AddDbContext<PublishDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("Default"), o =>
                {
                    o.UseCompatibilityLevel(120);
                });
            }).AddUnitOfWork<PublishDbContext>();

            services.AddServiceCollectionExtensions(configuration,
                typeof(Published.Core.Entities.Job).Assembly,
                typeof(Published.Core.Entities.Repo).Assembly,
                typeof(Published.Infrastructure.Services.MediumService).Assembly,
                typeof(PostIQ.Core.AI.LLM.GeminiLlmClient).Assembly,
                typeof(Published.Application.Models.JobModel).Assembly
            );

            return services;
        }
    }
}
