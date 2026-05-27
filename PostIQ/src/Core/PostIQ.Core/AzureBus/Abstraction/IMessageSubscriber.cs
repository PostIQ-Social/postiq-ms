using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.AzureBus.Abstraction
{
    /// <summary>
    /// Pull-based message subscriber for on-demand (non-background) message consumption.
    /// Messages received via <c>Receive*</c> methods are automatically completed after
    /// deserialization (when using PeekLock mode).
    /// Use <c>Peek*</c> methods to inspect messages without removing them from the queue.
    /// </summary>
    public interface IMessageSubscriber
    {
        /// <summary>
        /// Receives and completes a single message from a queue.
        /// Returns <c>null</c> if no message is available within the wait time.
        /// </summary>
        Task<TMessage?> ReceiveFromQueueAsync<TMessage>(
            string queueLogicalName,
            TimeSpan? maxWaitTime = null,
            CancellationToken cancellationToken = default) where TMessage : class;

        /// <summary>
        /// Receives and completes a batch of messages from a queue.
        /// </summary>
        Task<IReadOnlyList<TMessage>> ReceiveBatchFromQueueAsync<TMessage>(
            string queueLogicalName,
            int maxMessages = 10,
            TimeSpan? maxWaitTime = null,
            CancellationToken cancellationToken = default) where TMessage : class;

        /// <summary>
        /// Peeks at the next message in a queue without removing it.
        /// </summary>
        Task<TMessage?> PeekFromQueueAsync<TMessage>(
            string queueLogicalName,
            CancellationToken cancellationToken = default) where TMessage : class;

        /// <summary>
        /// Peeks at a batch of messages in a queue without removing them.
        /// </summary>
        Task<IReadOnlyList<TMessage>> PeekBatchFromQueueAsync<TMessage>(
            string queueLogicalName,
            int maxMessages = 10,
            CancellationToken cancellationToken = default) where TMessage : class;

        /// <summary>
        /// Receives and completes a single message from a topic subscription.
        /// Returns <c>null</c> if no message is available within the wait time.
        /// </summary>
        Task<TMessage?> ReceiveFromSubscriptionAsync<TMessage>(
            string topicLogicalName,
            string subscriptionLogicalName,
            TimeSpan? maxWaitTime = null,
            CancellationToken cancellationToken = default) where TMessage : class;

        /// <summary>
        /// Receives and completes a batch of messages from a topic subscription.
        /// </summary>
        Task<IReadOnlyList<TMessage>> ReceiveBatchFromSubscriptionAsync<TMessage>(
            string topicLogicalName,
            string subscriptionLogicalName,
            int maxMessages = 10,
            TimeSpan? maxWaitTime = null,
            CancellationToken cancellationToken = default) where TMessage : class;

        /// <summary>
        /// Peeks at the next message in a topic subscription without removing it.
        /// </summary>
        Task<TMessage?> PeekFromSubscriptionAsync<TMessage>(
            string topicLogicalName,
            string subscriptionLogicalName,
            CancellationToken cancellationToken = default) where TMessage : class;

        /// <summary>
        /// Peeks at a batch of messages in a topic subscription without removing them.
        /// </summary>
        Task<IReadOnlyList<TMessage>> PeekBatchFromSubscriptionAsync<TMessage>(
            string topicLogicalName,
            string subscriptionLogicalName,
            int maxMessages = 10,
            CancellationToken cancellationToken = default) where TMessage : class;
    }
}
