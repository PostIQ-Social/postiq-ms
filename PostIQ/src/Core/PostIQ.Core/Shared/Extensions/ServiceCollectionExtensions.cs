using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace PostIQ.Core.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceCollectionExtensions(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (assemblies is null) throw new ArgumentNullException(nameof(assemblies));

            services
                // Register repositories & services only. Do NOT auto-register MediatR requests (Query/Command) as DI services.
                .RegisterBySuffix(assemblies, "Repository")
                .RegisterBySuffix(assemblies, "Service")

                // misc registrations
                .RegisterMiddlewares(assemblies)
                .RegisterAutoMapper(assemblies)
                .RegisterMediatR(assemblies)

                // ensure handler interfaces are explicitly wired (IRequestHandler<,>, INotificationHandler<>)
                .RegisterGenericHandlers(assemblies, typeof(IRequestHandler<,>))
                .RegisterGenericHandlers(assemblies, typeof(INotificationHandler<>))

                // hosted/background workers
                .RegisterHostedServices(assemblies);

            return services;
        }

        private static IServiceCollection RegisterBySuffix(this IServiceCollection services, Assembly[] assemblies, string suffix)
        {
            if (string.IsNullOrWhiteSpace(suffix)) return services;

            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(c => c.Where(t => t.Name.EndsWith(suffix, StringComparison.Ordinal)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }

        private static IServiceCollection RegisterHostedServices(this IServiceCollection services, Assembly[] assemblies)
        {
            var workers = GetConcreteTypes(assemblies)
                .Where(t => typeof(BackgroundService).IsAssignableFrom(t))
                .ToList();

            foreach (var worker in workers)
            {
                services.AddHostedService(provider => (BackgroundService)ActivatorUtilities.CreateInstance(provider, worker));
            }

            return services;
        }

        private static IServiceCollection RegisterMiddlewares(this IServiceCollection services, Assembly[] assemblies)
        {
            var middlewares = GetConcreteTypes(assemblies)
                .Where(t => t.Name.EndsWith("Middleware", StringComparison.Ordinal))
                .ToList();

            foreach (var mw in middlewares)
            {
                services.AddTransient(mw);
            }

            return services;
        }

        private static IServiceCollection RegisterAutoMapper(this IServiceCollection services, Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                assemblies = new[] { Assembly.GetExecutingAssembly() };
            IEnumerable<Assembly> assemblyEnumerable = assemblies;
            services.AddAutoMapper(assemblyEnumerable);

            return services;
        }

        private static IServiceCollection RegisterMediatR(this IServiceCollection services, Assembly[] assemblies)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));
            return services;
        }

        private static IServiceCollection RegisterGenericHandlers(this IServiceCollection services, Assembly[] assemblies, Type genericInterfaceDefinition)
        {
            if (genericInterfaceDefinition is null) return services;

            var handlers = GetConcreteTypes(assemblies)
                .Where(t => t.Name.EndsWith("Handler", StringComparison.Ordinal) &&
                            t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceDefinition))
                .ToList();

            foreach (var handler in handlers)
            {
                var interfaces = handler.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceDefinition);

                foreach (var iface in interfaces)
                {
                    services.AddScoped(iface, handler);
                }
            }

            return services;
        }

        private static IEnumerable<Type> GetConcreteTypes(Assembly[] assemblies)
        {
            if (assemblies is null) throw new ArgumentNullException(nameof(assemblies));

            foreach (var assembly in assemblies)
            {
                if (assembly is null) continue;

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types?.Where(t => t != null).Select(t => t!).ToArray() ?? Array.Empty<Type>();
                }

                foreach (var t in types)
                {
                    if (t is null) continue;
                    if (!t.IsClass || t.IsAbstract) continue;
                    yield return t;
                }
            }
        }
    }
}
