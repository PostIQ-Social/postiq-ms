using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.AzureBus.Abstraction
{
    /// <summary>
    /// Publishes messages to Azure Service Bus queues or topics.
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Send a single message to a queue by its logical name (key in config).
        /// </summary>
        Task SendToQueueAsync<TMessage>(
            string queueLogicalName,
            TMessage message,
            Action<ServiceBusMessage>? configureMessage = null,
            CancellationToken cancellationToken = default) where TMessage : class;

        /// <summary>
        /// Send a batch of messages to a queue by its logical name.
        /// </summary>
        Task SendBatchToQueueAsync<TMessage>(
            string queueLogicalName,
            IEnumerable<TMessage> messages,
            Action<ServiceBusMessage>? configureMessage = null,
            CancellationToken cancellationToken = default) where TMessage : class;

        /// <summary>
        /// Send a single message to a topic by its logical name.
        /// </summary>
        Task SendToTopicAsync<TMessage>(
            string topicLogicalName,
            TMessage message,
            Action<ServiceBusMessage>? configureMessage = null,
            CancellationToken cancellationToken = default) where TMessage : class;

        /// <summary>
        /// Send a batch of messages to a topic by its logical name.
        /// </summary>
        Task SendBatchToTopicAsync<TMessage>(
            string topicLogicalName,
            IEnumerable<TMessage> messages,
            Action<ServiceBusMessage>? configureMessage = null,
            CancellationToken cancellationToken = default) where TMessage : class;

        /// <summary>
        /// Schedule a message to be enqueued at a specific UTC time on a queue.
        /// </summary>
        Task<long> ScheduleMessageToQueueAsync<TMessage>(
            string queueLogicalName,
            TMessage message,
            DateTimeOffset scheduledEnqueueTime,
            Action<ServiceBusMessage>? configureMessage = null,
            CancellationToken cancellationToken = default) where TMessage : class;

        /// <summary>
        /// Schedule a message to be enqueued at a specific UTC time on a topic.
        /// </summary>
        Task<long> ScheduleMessageToTopicAsync<TMessage>(
            string topicLogicalName,
            TMessage message,
            DateTimeOffset scheduledEnqueueTime,
            Action<ServiceBusMessage>? configureMessage = null,
            CancellationToken cancellationToken = default) where TMessage : class;

        /// <summary>
        /// Cancel a previously scheduled message on a queue.
        /// </summary>
        Task CancelScheduledQueueMessageAsync(
            string queueLogicalName,
            long sequenceNumber,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancel a previously scheduled message on a topic.
        /// </summary>
        Task CancelScheduledTopicMessageAsync(
            string topicLogicalName,
            long sequenceNumber,
            CancellationToken cancellationToken = default);
    }
}
