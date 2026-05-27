using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PostIQ.Core.HttpClientService.Configuration;
using PostIQ.Core.HttpClientService.Services;

namespace PostIQ.Core.HttpClientService.Extensions
{
    /// <summary>
    /// Extension methods to register the generic HTTP client and named clients from configuration.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Binds <see cref="HttpClientSettings"/> from configuration (optional) and registers options.
        /// </summary>
        public static IServiceCollection AddHttpClientSettings(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                services.Configure<HttpClientSettings>(configuration.GetSection(HttpClientSettings.SectionName));
            }
            else
            {
                services.AddOptions<HttpClientSettings>();
            }

            return services;
        }

        /// <summary>
        /// Registers the generic HTTP client service and named HttpClient instances from the provided configuration.
        /// Returns the last registered <see cref="IHttpClientBuilder"/> (useful for additional per-app configuration).
        /// </summary>
        public static IHttpClientBuilder AddHttpClientService(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            services.AddHttpClientSettings(configuration);

            // Register the concrete client service implementation
            services.AddSingleton<IBaseHttpClientService, BaseHttpClientService>();

            // Bind configuration to object
            var settings = new HttpClientSettings();
            configuration.GetSection(HttpClientSettings.SectionName).Bind(settings);

            if (settings.Clients == null || settings.Clients.Count == 0)
            {
                settings.Clients = new Dictionary<string, HttpClientOptions>(StringComparer.Ordinal)
                {
                    ["Default"] = new HttpClientOptions()
                };
            }

            IHttpClientBuilder? lastBuilder = null;

            foreach (var (name, opts) in settings.Clients)
            {
                var options = opts ?? new HttpClientOptions();

                var builder = services.AddHttpClient(name, (sp, client) =>
                {
                    if (!string.IsNullOrWhiteSpace(options.BaseAddress))
                    {
                        client.BaseAddress = new Uri(options.BaseAddress, UriKind.RelativeOrAbsolute);
                    }

                    if (options.Timeout.HasValue)
                    {
                        client.Timeout = options.Timeout.Value;
                    }

                    if (options.DefaultHeaders != null)
                    {
                        foreach (var header in options.DefaultHeaders)
                        {
                            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                });

                // Configure tuned SocketsHttpHandler for common high-throughput scenarios
                builder.ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new SocketsHttpHandler
                    {
                        PooledConnectionLifetime = TimeSpan.FromMinutes(15),
                        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
                        MaxConnectionsPerServer = 10,
                        EnableMultipleHttp2Connections = true
                    };

                    // Apply MaxResponseContentBufferSize if present (bounded by int.MaxValue)
                    if (options.MaxResponseContentBufferSize.HasValue && options.MaxResponseContentBufferSize.Value > 0)
                    {
                        try
                        {
                            handler.MaxResponseDrainSize = (int)Math.Min(options.MaxResponseContentBufferSize.Value, int.MaxValue);
                        }
                        catch
                        {
                            // Best-effort; ignore if assignment fails on platform
                        }
                    }

                    return handler;
                });

                lastBuilder = builder;
            }

            // lastBuilder is guaranteed non-null because we ensured at least one client above
            return lastBuilder!;
        }

        /// <summary>
        /// Convenience: register the generic client service and a single named HttpClient with custom configuration.
        /// </summary>
        public static IHttpClientBuilder AddHttpClientNamedService(
            this IServiceCollection services,
            string clientName,
            Action<System.Net.Http.HttpClient> configureClient,
            Action<SocketsHttpHandler>? configureHandler = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (string.IsNullOrWhiteSpace(clientName)) throw new ArgumentException("clientName must be provided", nameof(clientName));
            if (configureClient == null) throw new ArgumentNullException(nameof(configureClient));

            services.AddSingleton<IBaseHttpClientService, BaseHttpClientService>();

            var builder = services.AddHttpClient(clientName, configureClient);

            builder.ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new SocketsHttpHandler
                {
                    PooledConnectionLifetime = TimeSpan.FromMinutes(15),
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
                    MaxConnectionsPerServer = 10,
                    EnableMultipleHttp2Connections = true
                };

                configureHandler?.Invoke(handler);
                return handler;
            });

            return builder;
        }
    }
}
