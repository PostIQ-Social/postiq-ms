using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.AzureBus.Configuration
{
    /// <summary>
    /// Root configuration for Azure Service Bus. Bind to "AzureServiceBus" in appsettings.json.
    /// </summary>
    public sealed class ServiceBusOptions
    {
        public const string SectionName = "AzureServiceBus";

        /// <summary>
        /// Fully-qualified namespace (e.g. "mynamespace.servicebus.windows.net").
        /// Used with DefaultAzureCredential when ConnectionString is not set.
        /// </summary>
        public string? FullyQualifiedNamespace { get; set; }

        /// <summary>
        /// Primary connection string. Takes precedence over FullyQualifiedNamespace.
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Transport type for the underlying AMQP connection.
        /// </summary>
        public ServiceBusTransportType TransportType { get; set; } = ServiceBusTransportType.AmqpTcp;

        /// <summary>
        /// Global retry options applied to all operations unless overridden per queue/topic.
        /// </summary>
        public RetryOptions Retry { get; set; } = new();

        /// <summary>
        /// Named queue configurations. Key = logical name used in code.
        /// </summary>
        public Dictionary<string, QueueOptions> Queues { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Named topic configurations. Key = logical name used in code.
        /// </summary>
        public Dictionary<string, TopicOptions> Topics { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Whether to enable Service Bus health checks.
        /// </summary>
        public bool EnableHealthChecks { get; set; } = true;

        /// <summary>
        /// Timeout in seconds for health check probes.
        /// </summary>
        public int HealthCheckTimeoutSeconds { get; set; } = 10;
    }
}
