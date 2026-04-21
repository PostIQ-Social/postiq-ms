using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using PostIQ.Core.AzureBus.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.AzureBus.HelthChecks
{
    /// <summary>
    /// Health check that verifies connectivity to Azure Service Bus
    /// by confirming the client can be created and is not closed.
    /// </summary>
    public sealed class ServiceBusHealthCheck : IHealthCheck
    {
        private readonly ServiceBusClientFactory _factory;
        private readonly ILogger<ServiceBusHealthCheck> _logger;

        public ServiceBusHealthCheck(ServiceBusClientFactory factory, ILogger<ServiceBusHealthCheck> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var client = _factory.Client;

                if (client.IsClosed)
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy(
                        "Service Bus client is closed."));
                }

                return Task.FromResult(HealthCheckResult.Healthy(
                    $"Connected to {client.FullyQualifiedNamespace}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service Bus health check failed");
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "Failed to connect to Azure Service Bus.", ex));
            }
        }
    }
}
