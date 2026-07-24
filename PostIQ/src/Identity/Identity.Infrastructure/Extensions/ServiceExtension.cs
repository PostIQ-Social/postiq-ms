using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using PostIQ.Identity.Data;
using PostIQ.Identity.Options;
using PostIQ.Identity.Services;
using System.Text;
using System.Threading.RateLimiting;

namespace Identity.Infrastructure.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddServiceExtension(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<IdentityDbContext>(o => o.UseSqlServer(configuration.GetConnectionString("Default")));

            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
            services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));

            var jwtSection = configuration.GetSection(JwtOptions.SectionName);
            var signingKey = jwtSection.Get<JwtOptions>()?.SigningKey
                ?? throw new InvalidOperationException("Jwt:SigningKey is required.");
            var signingBytes = Encoding.UTF8.GetBytes(signingKey);
            if (signingBytes.Length < 32)
                throw new InvalidOperationException("Jwt:SigningKey must be at least 32 UTF-8 bytes.");

            services.AddSingleton<PasswordHasherService>();
            services.AddSingleton<TotpService>();
            services.AddSingleton<JwtTokenService>();
            services.AddScoped<AuthService>();



            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity Service", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization: Bearer {token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });
                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
                });
            });

            services.AddRateLimiter(o =>
            {
                o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                o.AddPolicy("auth", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 20,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));
            });


            return services;
        }
    }
}
