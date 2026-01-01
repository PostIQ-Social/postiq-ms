using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostIQ.Core.Database.Extension;
using User.Application.Mappers;
using User.Core.Persistence;

namespace User.Infrastructure.Extensions
{
    public static class DbContextExtension
    {
        public static IServiceCollection AddDbContextExtension(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration["DefaultConnection"];            
            services.AddDbContext<UserDBContext>(options =>
            {
                options.UseSqlServer(connectionString, o => o.UseCompatibilityLevel(120));
            }).AddUnitOfWork<UserDBContext>();

            return services;
        }
    }
}
