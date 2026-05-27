using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostIQ.Core.Database.Extension;
using Published.Core.Persistence;

namespace Published.Infrastructure.Extension
{
    public static class DbContextExtension
    {
        public static IServiceCollection AddDbContextExtension(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration["DefaultConnection"];
            services.AddDbContext<PublishDbContext>(options =>
            {
                options.UseSqlServer(connectionString, o =>
                {
                    o.UseCompatibilityLevel(120);
                });
            }).AddUnitOfWork<PublishDbContext>();

            return services;
        }
    }
}
