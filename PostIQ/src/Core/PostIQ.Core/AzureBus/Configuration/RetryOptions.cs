using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.AzureBus.Configuration
{
    public sealed class RetryOptions
    {
        /// <summary>
        /// Retry mode: Fixed or Exponential.
        /// </summary>
        public ServiceBusRetryMode Mode { get; set; } = ServiceBusRetryMode.Exponential;

        /// <summary>
        /// Maximum number of retry attempts before giving up.
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Delay between retry attempts (seconds).
        /// </summary>
        public double DelaySeconds { get; set; } = 0.8;

        /// <summary>
        /// Maximum delay between retries (seconds). Applies to exponential backoff.
        /// </summary>
        public double MaxDelaySeconds { get; set; } = 60;

        /// <summary>
        /// Maximum duration to wait for an operation to complete (seconds).
        /// </summary>
        public double TryTimeoutSeconds { get; set; } = 60;

        internal ServiceBusRetryOptions ToServiceBusRetryOptions() => new()
        {
            Mode = Mode,
            MaxRetries = MaxRetries,
            Delay = TimeSpan.FromSeconds(DelaySeconds),
            MaxDelay = TimeSpan.FromSeconds(MaxDelaySeconds),
            TryTimeout = TimeSpan.FromSeconds(TryTimeoutSeconds)
        };
    }
}
