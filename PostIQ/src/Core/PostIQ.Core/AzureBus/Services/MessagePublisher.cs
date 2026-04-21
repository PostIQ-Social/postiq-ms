using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using PostIQ.Core.AzureBus.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.AzureBus.Services
{
    public sealed class MessagePublisher : IMessagePublisher
    {
        private readonly ServiceBusClientFactory _factory;
        private readonly IMessageSerializer _serializer;
        private readonly ILogger<MessagePublisher> _logger;

        public MessagePublisher(
            ServiceBusClientFactory factory,
            IMessageSerializer serializer,
            ILogger<MessagePublisher> logger)
        {
            _factory = factory;
            _serializer = serializer;
            _logger = logger;
        }

        public async Task SendToQueueAsync<TMessage>(
            string queueLogicalName,
            TMessage message,
            Action<ServiceBusMessage>? configureMessage = null,
            CancellationToken cancellationToken = default) where TMessage : class
        {
            var sender = _factory.GetOrCreateQueueSender(queueLogicalName);
            var sbMessage = CreateMessage(message);
            configureMessage?.Invoke(sbMessage);

            _logger.LogDebug("Sending message to queue '{Queue}', MessageId={MessageId}",
                queueLogicalName, sbMessage.MessageId);

            await sender.SendMessageAsync(sbMessage, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Message sent to queue '{Queue}', MessageId={MessageId}",
                queueLogicalName, sbMessage.MessageId);
        }

        public async Task SendBatchToQueueAsync<TMessage>(
            string queueLogicalName,
            IEnumerable<TMessage> messages,
            Action<ServiceBusMessage>? configureMessage = null,
            CancellationToken cancellationToken = default) where TMessage : class
        {
            var sender = _factory.GetOrCreateQueueSender(queueLogicalName);
            await SendBatchInternalAsync(sender, messages, queueLogicalName, configureMessage, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task SendToTopicAsync<TMessage>(
            string topicLogicalName,
            TMessage message,
            Action<ServiceBusMessage>? configureMessage = null,
            CancellationToken cancellationToken = default) where TMessage : class
        {
            var sender = _factory.GetOrCreateTopicSender(topicLogicalName);
            var sbMessage = CreateMessage(message);
            configureMessage?.Invoke(sbMessage);

            _logger.LogDebug("Sending message to topic '{Topic}', MessageId={MessageId}",
                topicLogicalName, sbMessage.MessageId);

            await sender.SendMessageAsync(sbMessage, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Message sent to topic '{Topic}', MessageId={MessageId}",
                topicLogicalName, sbMessage.MessageId);
        }

        public async Task SendBatchToTopicAsync<TMessage>(
            string topicLogicalName,
            IEnumerable<TMessage> messages,
            Action<ServiceBusMessage>? configureMessage = null,
            CancellationToken cancellationToken = default) where TMessage : class
        {
            var sender = _factory.GetOrCreateTopicSender(topicLogicalName);
            await SendBatchInternalAsync(sender, messages, topicLogicalName, configureMessage, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<long> ScheduleMessageToQueueAsync<TMessage>(
            string queueLogicalName,
            TMessage message,
            DateTimeOffset scheduledEnqueueTime,
            Action<ServiceBusMessage>? configureMessage = null,
            CancellationToken cancellationToken = default) where TMessage : class
        {
            var sender = _factory.GetOrCreateQueueSender(queueLogicalName);
            var sbMessage = CreateMessage(message);
            configureMessage?.Invoke(sbMessage);

            var sequenceNumber = await sender
                .ScheduleMessageAsync(sbMessage, scheduledEnqueueTime, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "Scheduled message to queue '{Queue}' at {ScheduledTime}, SequenceNumber={Seq}",
                queueLogicalName, scheduledEnqueueTime, sequenceNumber);

            return sequenceNumber;
        }

        public async Task<long> ScheduleMessageToTopicAsync<TMessage>(
            string topicLogicalName,
            TMessage message,
            DateTimeOffset scheduledEnqueueTime,
            Action<ServiceBusMessage>? configureMessage = null,
            CancellationToken cancellationToken = default) where TMessage : class
        {
            var sender = _factory.GetOrCreateTopicSender(topicLogicalName);
            var sbMessage = CreateMessage(message);
            configureMessage?.Invoke(sbMessage);

            var sequenceNumber = await sender
                .ScheduleMessageAsync(sbMessage, scheduledEnqueueTime, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "Scheduled message to topic '{Topic}' at {ScheduledTime}, SequenceNumber={Seq}",
                topicLogicalName, scheduledEnqueueTime, sequenceNumber);

            return sequenceNumber;
        }

        public async Task CancelScheduledQueueMessageAsync(
            string queueLogicalName,
            long sequenceNumber,
            CancellationToken cancellationToken = default)
        {
            var sender = _factory.GetOrCreateQueueSender(queueLogicalName);
            await sender.CancelScheduledMessageAsync(sequenceNumber, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Cancelled scheduled message on queue '{Queue}', SequenceNumber={Seq}",
                queueLogicalName, sequenceNumber);
        }

        public async Task CancelScheduledTopicMessageAsync(
            string topicLogicalName,
            long sequenceNumber,
            CancellationToken cancellationToken = default)
        {
            var sender = _factory.GetOrCreateTopicSender(topicLogicalName);
            await sender.CancelScheduledMessageAsync(sequenceNumber, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Cancelled scheduled message on topic '{Topic}', SequenceNumber={Seq}",
                topicLogicalName, sequenceNumber);
        }

        private ServiceBusMessage CreateMessage<TMessage>(TMessage message) where TMessage : class
        {
            var sbMessage = new ServiceBusMessage(_serializer.Serialize(message))
            {
                ContentType = _serializer.ContentType,
                MessageId = Guid.NewGuid().ToString()
            };

            sbMessage.ApplicationProperties["MessageType"] = typeof(TMessage).FullName;
            return sbMessage;
        }

        private async Task SendBatchInternalAsync<TMessage>(
            ServiceBusSender sender,
            IEnumerable<TMessage> messages,
            string destination,
            Action<ServiceBusMessage>? configureMessage,
            CancellationToken cancellationToken) where TMessage : class
        {
            var messageList = messages.ToList();
            var batch = await sender.CreateMessageBatchAsync(cancellationToken).ConfigureAwait(false);
            var batchCount = 0;

            foreach (var message in messageList)
            {
                var sbMessage = CreateMessage(message);
                configureMessage?.Invoke(sbMessage);

                if (!batch.TryAddMessage(sbMessage))
                {
                    if (batch.Count == 0)
                    {
                        throw new InvalidOperationException(
                            "A single message is too large to fit in a batch.");
                    }

                    await sender.SendMessagesAsync(batch, cancellationToken).ConfigureAwait(false);
                    batchCount += batch.Count;

                    batch.Dispose();
                    batch = await sender.CreateMessageBatchAsync(cancellationToken).ConfigureAwait(false);

                    if (!batch.TryAddMessage(sbMessage))
                    {
                        throw new InvalidOperationException(
                            "A single message is too large to fit in a batch.");
                    }
                }
            }

            if (batch.Count > 0)
            {
                await sender.SendMessagesAsync(batch, cancellationToken).ConfigureAwait(false);
                batchCount += batch.Count;
            }

            batch.Dispose();

            _logger.LogInformation("Sent {Count} messages in batches to '{Destination}'",
                batchCount, destination);
        }
    }
}
