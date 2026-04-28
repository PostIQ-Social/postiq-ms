using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PostIQ.Core.Middlewares.Jwt
{
    public static class JwtAuthExtensions
    {
        /// <summary>
        /// Registers <see cref="JwtAuthOptions"/> from configuration
        /// and adds authorization services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">App configuration (reads the "Jwt" section by default).</param>
        /// <param name="sectionName">Override the config section name if needed.</param>
        public static IServiceCollection AddJwtAuth(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = JwtAuthOptions.SectionName)
        {
            services.Configure<JwtAuthOptions>(configuration.GetSection(sectionName));
            services.AddAuthorization();
            return services;
        }

        /// <summary>
        /// Adds routing (if not already added) and the custom JWT authorization middleware
        /// to the request pipeline.
        /// </summary>
        public static IApplicationBuilder UseJwtAuth(this IApplicationBuilder app)
        {
            //app.UseRouting();
            app.UseMiddleware<JwtAuthorizationMiddleware>();
            return app;
        }
    }
}
