using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.AzureBus.Configuration
{
    public sealed class QueueOptions
    {
        /// <summary>
        /// The actual Azure Service Bus queue name.
        /// </summary>
        public string QueueName { get; set; } = string.Empty;

        // — Sender options ——————————————————————————————————————————————————————

        /// <summary>
        /// Identifier used for diagnostics when sending messages.
        /// </summary>
        public string? SenderIdentifier { get; set; }

        // — Processor / Receiver options ————————————————————————————————————————

        /// <summary>
        /// Maximum number of concurrent calls to the message handler.
        /// </summary>
        public int MaxConcurrentCalls { get; set; } = 1;

        /// <summary>
        /// Whether the processor automatically completes messages after the handler returns.
        /// </summary>
        public bool AutoCompleteMessages { get; set; } = true;

        /// <summary>
        /// Number of messages the processor prefetches for processing.
        /// </summary>
        public int PrefetchCount { get; set; } = 0;

        /// <summary>
        /// Receive mode: PeekLock (default, safe) or ReceiveAndDelete.
        /// </summary>
        public ServiceBusReceiveMode ReceiveMode { get; set; } = ServiceBusReceiveMode.PeekLock;

        /// <summary>
        /// Maximum duration the receiver will wait for a message (seconds).
        /// </summary>
        public double MaxWaitTimeSeconds { get; set; } = 60;

        /// <summary>
        /// Maximum time the message lock is held before auto-renewal (seconds).
        /// Set to 0 to disable auto-renewal.
        /// </summary>
        public double MaxAutoLockRenewalDurationSeconds { get; set; } = 300;

        /// <summary>
        /// Sub-queue to connect to (None, DeadLetter, TransferDeadLetter).
        /// </summary>
        public SubQueue SubQueue { get; set; } = SubQueue.None;

        /// <summary>
        /// Enable session-based processing for this queue.
        /// </summary>
        public bool EnableSessions { get; set; } = false;

        /// <summary>
        /// Maximum number of concurrent sessions when sessions are enabled.
        /// </summary>
        public int MaxConcurrentSessions { get; set; } = 8;

        /// <summary>
        /// Maximum concurrent calls per session when sessions are enabled.
        /// </summary>
        public int MaxConcurrentCallsPerSession { get; set; } = 1;

        /// <summary>
        /// Idle timeout for sessions (seconds). 0 = no timeout.
        /// </summary>
        public double SessionIdleTimeoutSeconds { get; set; } = 0;

        /// <summary>
        /// Maximum messages to receive per batch (for batch receive operations).
        /// </summary>
        public int MaxMessagesBatch { get; set; } = 10;

        /// <summary>
        /// Maximum delivery count before sending to dead-letter queue.
        /// Used for manual dead-letter threshold checks.
        /// </summary>
        public int MaxDeliveryCount { get; set; } = 10;

        /// <summary>
        /// Whether to automatically start the processor on application startup.
        /// </summary>
        public bool AutoStartProcessor { get; set; } = true;
    }
}
