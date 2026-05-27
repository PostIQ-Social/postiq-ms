using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostIQ.Core.AzureBus.Abstraction;
using PostIQ.Core.AzureBus.Configuration;
using PostIQ.Core.AzureBus.HelthChecks;
using PostIQ.Core.AzureBus.Serialization;
using PostIQ.Core.AzureBus.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using static PostIQ.Core.AzureBus.Abstraction.IMessageHandler;

namespace PostIQ.Core.AzureBus.Extension
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the Azure Service Bus core infrastructure: configuration binding,
        /// client factory, publisher, and serializer.
        /// </summary>
        public static IServiceCollection AddAzureServiceBus(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<ServiceBusOptions>? configure = null)
        {
            var section = configuration.GetSection(ServiceBusOptions.SectionName);
            services.Configure<ServiceBusOptions>(section);

            if (configure is not null)
            {
                services.PostConfigure(configure);
            }

            services.AddSingleton<ServiceBusClientFactory>();
            services.AddSingleton<IMessagePublisher, MessagePublisher>();
            services.AddSingleton<IMessageSubscriber, MessageSubscriber>();
            services.TryAddSerializer();

            var options = section.Get<ServiceBusOptions>() ?? new ServiceBusOptions();
            configure?.Invoke(options);

            if (options.EnableHealthChecks)
            {
                services.AddHealthChecks()
                    .AddCheck<ServiceBusHealthCheck>(
                        "azure-service-bus",
                        tags: new[] { "ready", "servicebus" },
                        timeout: TimeSpan.FromSeconds(options.HealthCheckTimeoutSeconds));
            }

            return services;
        }

        /// <summary>
        /// Registers a message handler for a specific queue. This automatically starts a
        /// background processor that listens for messages and dispatches them to the handler.
        /// </summary>
        /// <typeparam name="TMessage">Message DTO type.</typeparam>
        /// <typeparam name="THandler">Handler implementation type.</typeparam>
        public static IServiceCollection AddQueueHandler<TMessage, THandler>(
            this IServiceCollection services,
            string queueLogicalName)
            where TMessage : class
            where THandler : class, IMessageHandler<TMessage>
        {
            services.AddScoped<IMessageHandler<TMessage>, THandler>();

            services.AddSingleton<IHostedService>(sp =>
            {
                var factory = sp.GetRequiredService<ServiceBusClientFactory>();
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var serializer = sp.GetRequiredService<IMessageSerializer>();
                var options = sp.GetRequiredService<IOptions<ServiceBusOptions>>();
                var logger = sp.GetRequiredService<ILogger<QueueListenerHostedService<TMessage>>>();

                return new QueueListenerHostedService<TMessage>(
                    factory, scopeFactory, serializer, options, logger, queueLogicalName);
            });

            return services;
        }

        /// <summary>
        /// Registers a session-aware message handler for a specific queue.
        /// </summary>
        public static IServiceCollection AddSessionQueueHandler<TMessage, THandler>(
            this IServiceCollection services,
            string queueLogicalName)
            where TMessage : class
            where THandler : class, ISessionMessageHandler<TMessage>
        {
            services.AddScoped<ISessionMessageHandler<TMessage>, THandler>();

            services.AddSingleton<IHostedService>(sp =>
            {
                var factory = sp.GetRequiredService<ServiceBusClientFactory>();
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var serializer = sp.GetRequiredService<IMessageSerializer>();
                var options = sp.GetRequiredService<IOptions<ServiceBusOptions>>();
                var logger = sp.GetRequiredService<ILogger<QueueListenerHostedService<TMessage>>>();

                return new QueueListenerHostedService<TMessage>(
                    factory, scopeFactory, serializer, options, logger, queueLogicalName);
            });

            return services;
        }

        /// <summary>
        /// Registers a message handler for a specific topic subscription.
        /// </summary>
        public static IServiceCollection AddSubscriptionHandler<TMessage, THandler>(
            this IServiceCollection services,
            string topicLogicalName,
            string subscriptionLogicalName)
            where TMessage : class
            where THandler : class, IMessageHandler<TMessage>
        {
            services.AddScoped<IMessageHandler<TMessage>, THandler>();

            services.AddSingleton<IHostedService>(sp =>
            {
                var factory = sp.GetRequiredService<ServiceBusClientFactory>();
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var serializer = sp.GetRequiredService<IMessageSerializer>();
                var options = sp.GetRequiredService<IOptions<ServiceBusOptions>>();
                var logger = sp.GetRequiredService<ILogger<TopicListenerHostedService<TMessage>>>();

                return new TopicListenerHostedService<TMessage>(
                    factory, scopeFactory, serializer, options, logger,
                    topicLogicalName, subscriptionLogicalName);
            });

            return services;
        }

        /// <summary>
        /// Replaces the default System.Text.Json serializer with a custom one.
        /// </summary>
        public static IServiceCollection UseCustomSerializer<TSerializer>(
            this IServiceCollection services)
            where TSerializer : class, IMessageSerializer
        {
            var existing = services.FirstOrDefault(d => d.ServiceType == typeof(IMessageSerializer));
            if (existing != null) services.Remove(existing);

            services.AddSingleton<IMessageSerializer, TSerializer>();
            return services;
        }

        private static void TryAddSerializer(this IServiceCollection services)
        {
            if (services.Any(d => d.ServiceType == typeof(IMessageSerializer)))
                return;

            services.AddSingleton<IMessageSerializer, SystemTextJsonMessageSerializer>();
        }
    }
}
