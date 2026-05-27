using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostIQ.Core.AzureBus.Abstraction;
using PostIQ.Core.AzureBus.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.AzureBus.Services
{
    /// <summary>
    /// Pull-based subscriber that uses <see cref="ServiceBusReceiver" /> for on-demand
    /// message consumption - no background processor required.
    /// </summary>
    public sealed class MessageSubscriber : IMessageSubscriber
    {
        private readonly ServiceBusClientFactory _factory;
        private readonly IMessageSerializer _serializer;
        private readonly ServiceBusOptions _options;
        private readonly ILogger<MessageSubscriber> _logger;

        public MessageSubscriber(
            ServiceBusClientFactory factory,
            IMessageSerializer serializer,
            IOptions<ServiceBusOptions> options,
            ILogger<MessageSubscriber> logger)
        {
            _factory = factory;
            _serializer = serializer;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<TMessage?> ReceiveFromQueueAsync<TMessage>(
            string queueLogicalName,
            TimeSpan? maxWaitTime = null,
            CancellationToken cancellationToken = default) where TMessage : class
        {
            var receiver = _factory.GetOrCreateQueueReceiver(queueLogicalName);
            var waitTime = maxWaitTime ?? ResolveQueueWaitTime(queueLogicalName);

            var received = await receiver
                .ReceiveMessageAsync(waitTime, cancellationToken)
                .ConfigureAwait(false);

            if (received is null) return null;

            var message = _serializer.Deserialze<TMessage>(received.Body);
            await CompleteIfPeekLock(receiver, received, cancellationToken).ConfigureAwait(false);

            _logger.LogDebug(
                "Received message from queue '{Queue}', MessageId={MessageId}",
                queueLogicalName, received.MessageId);

            return message;
        }

        public async Task<IReadOnlyList<TMessage>> ReceiveBatchFromQueueAsync<TMessage>(
            string queueLogicalName,
            int maxMessages = 10,
            TimeSpan? maxWaitTime = null,
            CancellationToken cancellationToken = default) where TMessage : class
        {
            var receiver = _factory.GetOrCreateQueueReceiver(queueLogicalName);
            var waitTime = maxWaitTime ?? ResolveQueueWaitTime(queueLogicalName);

            var received = await receiver
                .ReceiveMessagesAsync(maxMessages, waitTime, cancellationToken)
                .ConfigureAwait(false);

            var results = new List<TMessage>(received.Count);

            foreach (var msg in received)
            {
                results.Add(_serializer.Deserialze<TMessage>(msg.Body));
                await CompleteIfPeekLock(receiver, msg, cancellationToken).ConfigureAwait(false);
            }

            _logger.LogDebug("Received {Count} messages from queue '{Queue}'",
                results.Count, queueLogicalName);

            return results;
        }

        public async Task<TMessage?> PeekFromQueueAsync<TMessage>(
            string queueLogicalName,
            CancellationToken cancellationToken = default) where TMessage : class
        {
            var receiver = _factory.GetOrCreateQueueReceiver(queueLogicalName);

            var peeked = await receiver
                .PeekMessageAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (peeked is null) return null;

            return _serializer.Deserialze<TMessage>(peeked.Body);
        }

        public async Task<IReadOnlyList<TMessage>> PeekBatchFromQueueAsync<TMessage>(
            string queueLogicalName,
            int maxMessages = 10,
            CancellationToken cancellationToken = default) where TMessage : class
        {
            var receiver = _factory.GetOrCreateQueueReceiver(queueLogicalName);

            var peeked = await receiver
                .PeekMessagesAsync(maxMessages, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return peeked
                .Select(m => _serializer.Deserialze<TMessage>(m.Body))
                .ToList();
        }

        public async Task<TMessage?> ReceiveFromSubscriptionAsync<TMessage>(
            string topicLogicalName,
            string subscriptionLogicalName,
            TimeSpan? maxWaitTime = null,
            CancellationToken cancellationToken = default) where TMessage : class
        {
            var receiver = _factory.GetOrCreateSubscriptionReceiver(topicLogicalName, subscriptionLogicalName);
            var waitTime = maxWaitTime ?? ResolveSubscriptionWaitTime(topicLogicalName, subscriptionLogicalName);

            var received = await receiver
                .ReceiveMessageAsync(waitTime, cancellationToken)
                .ConfigureAwait(false);

            if (received is null) return null;

            var message = _serializer.Deserialze<TMessage>(received.Body);
            await CompleteIfPeekLock(receiver, received, cancellationToken).ConfigureAwait(false);

            _logger.LogDebug(
                "Received message from '{Topic}/{Sub}', MessageId={MessageId}",
                topicLogicalName, subscriptionLogicalName, received.MessageId);

            return message;
        }

        public async Task<IReadOnlyList<TMessage>> ReceiveBatchFromSubscriptionAsync<TMessage>(
            string topicLogicalName,
            string subscriptionLogicalName,
            int maxMessages = 10,
            TimeSpan? maxWaitTime = null,
            CancellationToken cancellationToken = default) where TMessage : class
        {
            var receiver = _factory.GetOrCreateSubscriptionReceiver(topicLogicalName, subscriptionLogicalName);
            var waitTime = maxWaitTime ?? ResolveSubscriptionWaitTime(topicLogicalName, subscriptionLogicalName);

            var received = await receiver
                .ReceiveMessagesAsync(maxMessages, waitTime, cancellationToken);

            var results = new List<TMessage>(received.Count);

            foreach (var msg in received)
            {
                results.Add(_serializer.Deserialze<TMessage>(msg.Body));
                await CompleteIfPeekLock(receiver, msg, cancellationToken).ConfigureAwait(false);
            }

            _logger.LogDebug("Received {Count} messages from '{Topic}/{Sub}'",
                results.Count, topicLogicalName, subscriptionLogicalName);

            return results;
        }

        public async Task<TMessage?> PeekFromSubscriptionAsync<TMessage>(
            string topicLogicalName,
            string subscriptionLogicalName,
            CancellationToken cancellationToken = default) where TMessage : class
        {
            var receiver = _factory.GetOrCreateSubscriptionReceiver(topicLogicalName, subscriptionLogicalName);

            var peeked = await receiver
                .PeekMessageAsync(cancellationToken: cancellationToken);

            if (peeked is null) return null;

            return _serializer.Deserialze<TMessage>(peeked.Body);
        }

        public async Task<IReadOnlyList<TMessage>> PeekBatchFromSubscriptionAsync<TMessage>(
            string topicLogicalName,
            string subscriptionLogicalName,
            int maxMessages = 10,
            CancellationToken cancellationToken = default) where TMessage : class
        {
            var receiver = _factory.GetOrCreateSubscriptionReceiver(topicLogicalName, subscriptionLogicalName);

            var peeked = await receiver
                .PeekMessagesAsync(maxMessages, cancellationToken: cancellationToken);

            return peeked
                .Select(m => _serializer.Deserialze<TMessage>(m.Body))
                .ToList();
        }

        private static async Task CompleteIfPeekLock(
            ServiceBusReceiver receiver, ServiceBusReceivedMessage message, CancellationToken ct)
        {
            if (receiver.ReceiveMode == ServiceBusReceiveMode.PeekLock)
            {
                await receiver.CompleteMessageAsync(message, ct).ConfigureAwait(false);
            }
        }

        private TimeSpan ResolveQueueWaitTime(string queueLogicalName)
        {
            if (_options.Queues.TryGetValue(queueLogicalName, out var q) && q.MaxWaitTimeSeconds > 0)
            {
                return TimeSpan.FromSeconds(q.MaxWaitTimeSeconds);
            }

            return TimeSpan.FromSeconds(5);
        }

        private TimeSpan ResolveSubscriptionWaitTime(string topicLogicalName, string subscriptionLogicalName)
        {
            if (_options.Topics.TryGetValue(topicLogicalName, out var t)
                && t.Subscriptions.TryGetValue(subscriptionLogicalName, out var s))
            {
                // SubscriptionOptions doesn't have MaxWaitTimeSeconds; use a sensible default
            }

            return TimeSpan.FromSeconds(5);
        }
    }
}
