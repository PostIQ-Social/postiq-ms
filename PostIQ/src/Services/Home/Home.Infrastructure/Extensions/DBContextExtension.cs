using Home.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostIQ.Core.Database.Extension;
using PostIQ.Core.Shared.Extensions;

namespace Home.Infrastructure.Extensions
{
    public static class DbContextExtension
    {
        public static IServiceCollection AddDbContextExtension(this IServiceCollection services, IConfiguration configuration)
        {
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
