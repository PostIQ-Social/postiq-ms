using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.AzureBus.Configuration
{
    public sealed class TopicOptions
    {
        /// <summary>
        /// The actual Azure Service Bus topic name.
        /// </summary>
        public string TopicName { get; set; } = string.Empty;

        /// <summary>
        /// Identifier used for diagnostics when sending messages.
        /// </summary>
        public string? SenderIdentifier { get; set; }

        /// <summary>
        /// Named subscription configurations. Key = logical name.
        /// </summary>
        public Dictionary<string, SubscriptionOptions> Subscriptions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    public sealed class SubscriptionOptions
    {
        /// <summary>
        /// The actual Azure Service Bus subscription name.
        /// </summary>
        public string SubscriptionName { get; set; } = string.Empty;

        /// <summary>
        /// Maximum number of concurrent calls to the message handler.
        /// </summary>
        public int MaxConcurrentCalls { get; set; } = 1;

        /// <summary>
        /// Whether the processor automatically completes messages after handler returns.
        /// </summary>
        public bool AutoCompleteMessages { get; set; } = true;

        /// <summary>
        /// Number of messages the processor prefetches.
        /// </summary>
        public int PrefetchCount { get; set; } = 0;

        /// <summary>
        /// Receive mode: PeekLock (default) or ReceiveAndDelete.
        /// </summary>
        public ServiceBusReceiveMode ReceiveMode { get; set; } = ServiceBusReceiveMode.PeekLock;

        /// <summary>
        /// Maximum time the message lock is held before auto-renewal (seconds).
        /// </summary>
        public double MaxAutoLockRenewalDurationSeconds { get; set; } = 300;

        /// <summary>
        /// Sub-queue to connect to (None, DeadLetter, TransferDeadLetter).
        /// </summary>
        public SubQueue SubQueue { get; set; } = SubQueue.None;

        /// <summary>
        /// Enable session-based processing for this subscription.
        /// </summary>
        public bool EnableSessions { get; set; } = false;

        /// <summary>
        /// Maximum number of concurrent sessions when sessions are enabled.
        /// </summary>
        public int MaxConcurrentSessions { get; set; } = 8;

        /// <summary>
        /// Maximum concurrent calls per session.
        /// </summary>
        public int MaxConcurrentCallsPerSession { get; set; } = 1;

        /// <summary>
        /// Idle timeout for sessions (seconds). 0 = no timeout.
        /// </summary>
        public double SessionIdleTimeoutSeconds { get; set; } = 0;

        /// <summary>
        /// Maximum messages per batch for batch receive operations.
        /// </summary>
        public int MaxMessagesBatch { get; set; } = 10;

        /// <summary>
        /// Whether to automatically start the processor on application startup.
        /// </summary>
        public bool AutoStartProcessor { get; set; } = true;
    }
}
