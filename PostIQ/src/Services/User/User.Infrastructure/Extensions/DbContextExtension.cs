using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostIQ.Core.Database.Extension;
using PostIQ.Core.Shared.Extensions;
using User.Core.Persistence;

namespace User.Infrastructure.Extensions
{
    public static class DbContextExtension
    {
        public static IServiceCollection AddDbContextExtension(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddServiceCollectionExtensions(configuration,
                typeof(User.Core.Entities.UserDetail).Assembly,
                typeof(User.Application.Handlers.GetUserByIdHandler).Assembly,
                typeof(User.Infrastructure.Repositories.UserRepository).Assembly
            );
            services.AddDbContext<UserDBContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("Default"), o =>
                {
                    o.UseCompatibilityLevel(120);
                    o.MigrationsAssembly("User.API");
                });
            }).AddUnitOfWork<UserDBContext>();

            return services;
        }
    }
}
