using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using PostIQ.Core.AzureBus.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.AzureBus.Services
{
    /// <summary>
    /// Thread-safe factory that creates and caches ServiceBusClient, senders, and processors.
    /// Implements IAsyncDisposable to cleanly shut down all cached resources.
    /// </summary>
    public sealed class ServiceBusClientFactory : IAsyncDisposable
    {
        private readonly ServiceBusOptions _options;
        private readonly Lazy<ServiceBusClient> _client;
        private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();
        private readonly ConcurrentDictionary<string, ServiceBusReceiver> _receivers = new();
        private readonly ConcurrentBag<ServiceBusProcessor> _processors = new();
        private readonly ConcurrentBag<ServiceBusSessionProcessor> _sessionProcessors = new();

        public ServiceBusClientFactory(IOptions<ServiceBusOptions> options)
        {
            _options = options.Value;
            _client = new Lazy<ServiceBusClient>(CreateClient);
        }

        public ServiceBusClient Client => _client.Value;

        public ServiceBusSender GetOrCreateQueueSender(string queueLogicalName)
        {
            var queueOptions = GetQueueOptions(queueLogicalName);
            var cacheKey = $"queue:{queueOptions.QueueName}";

            return _senders.GetOrAdd(cacheKey, _ =>
            {
                var senderOptions = new ServiceBusSenderOptions
                {
                    Identifier = queueOptions.SenderIdentifier ?? $"sender-{queueOptions.QueueName}"
                };
                return Client.CreateSender(queueOptions.QueueName, senderOptions);
            });
        }

        public ServiceBusSender GetOrCreateTopicSender(string topicLogicalName)
        {
            var topicOptions = GetTopicOptions(topicLogicalName);
            var cacheKey = $"topic:{topicOptions.TopicName}";

            return _senders.GetOrAdd(cacheKey, _ =>
            {
                var senderOptions = new ServiceBusSenderOptions
                {
                    Identifier = topicOptions.SenderIdentifier ?? $"sender-{topicOptions.TopicName}"
                };
                return Client.CreateSender(topicOptions.TopicName, senderOptions);
            });
        }

        public ServiceBusProcessor CreateQueueProcessor(string queueLogicalName)
        {
            var q = GetQueueOptions(queueLogicalName);

            var processorOptions = new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = q.MaxConcurrentCalls,
                AutoCompleteMessages = q.AutoCompleteMessages,
                PrefetchCount = q.PrefetchCount,
                ReceiveMode = q.ReceiveMode,
                MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(q.MaxAutoLockRenewalDurationSeconds),
                SubQueue = q.SubQueue,
                Identifier = $"processor-{q.QueueName}"
            };

            var processor = Client.CreateProcessor(q.QueueName, processorOptions);
            _processors.Add(processor);
            return processor;
        }

        public ServiceBusSessionProcessor CreateQueueSessionProcessor(string queueLogicalName)
        {
            var q = GetQueueOptions(queueLogicalName);

            var processorOptions = new ServiceBusSessionProcessorOptions
            {
                MaxConcurrentSessions = q.MaxConcurrentSessions,
                MaxConcurrentCallsPerSession = q.MaxConcurrentCallsPerSession,
                AutoCompleteMessages = q.AutoCompleteMessages,
                PrefetchCount = q.PrefetchCount,
                ReceiveMode = q.ReceiveMode,
                MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(q.MaxAutoLockRenewalDurationSeconds),
                SessionIdleTimeout = q.SessionIdleTimeoutSeconds > 0
                    ? TimeSpan.FromSeconds(q.SessionIdleTimeoutSeconds)
                    : null,
                Identifier = $"session-processor-{q.QueueName}"
            };

            var processor = Client.CreateSessionProcessor(q.QueueName, processorOptions);
            _sessionProcessors.Add(processor);
            return processor;
        }

        public ServiceBusProcessor CreateSubscriptionProcessor(string topicLogicalName, string subscriptionLogicalName)
        {
            var topic = GetTopicOptions(topicLogicalName);

            if (!topic.Subscriptions.TryGetValue(subscriptionLogicalName, out var sub))
            {
                throw new InvalidOperationException(
                    $"Subscription '{subscriptionLogicalName}' not found under topic '{topicLogicalName}' in configuration.");
            }

            var processorOptions = new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = sub.MaxConcurrentCalls,
                AutoCompleteMessages = sub.AutoCompleteMessages,
                PrefetchCount = sub.PrefetchCount,
                ReceiveMode = sub.ReceiveMode,
                MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(sub.MaxAutoLockRenewalDurationSeconds),
                SubQueue = sub.SubQueue,
                Identifier = $"processor-{topic.TopicName}-{sub.SubscriptionName}"
            };

            var processor = Client.CreateProcessor(topic.TopicName, sub.SubscriptionName, processorOptions);
            _processors.Add(processor);
            return processor;
        }

        public ServiceBusSessionProcessor CreateSubscriptionSessionProcessor(string topicLogicalName, string subscriptionLogicalName)
        {
            var topic = GetTopicOptions(topicLogicalName);

            if (!topic.Subscriptions.TryGetValue(subscriptionLogicalName, out var sub))
            {
                throw new InvalidOperationException(
                    $"Subscription '{subscriptionLogicalName}' not found under topic '{topicLogicalName}' in configuration.");
            }

            var processorOptions = new ServiceBusSessionProcessorOptions
            {
                MaxConcurrentSessions = sub.MaxConcurrentSessions,
                MaxConcurrentCallsPerSession = sub.MaxConcurrentCallsPerSession,
                AutoCompleteMessages = sub.AutoCompleteMessages,
                PrefetchCount = sub.PrefetchCount,
                ReceiveMode = sub.ReceiveMode,
                MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(sub.MaxAutoLockRenewalDurationSeconds),
                SessionIdleTimeout = sub.SessionIdleTimeoutSeconds > 0
                    ? TimeSpan.FromSeconds(sub.SessionIdleTimeoutSeconds)
                    : null,
                Identifier = $"session-processor-{topic.TopicName}-{sub.SubscriptionName}"
            };

            var processor = Client.CreateSessionProcessor(topic.TopicName, sub.SubscriptionName, processorOptions);
            _sessionProcessors.Add(processor);
            return processor;
        }

        public ServiceBusReceiver CreateQueueReceiver(string queueLogicalName)
        {
            var q = GetQueueOptions(queueLogicalName);

            var receiverOptions = new ServiceBusReceiverOptions
            {
                ReceiveMode = q.ReceiveMode,
                PrefetchCount = q.PrefetchCount,
                SubQueue = q.SubQueue,
                Identifier = $"receiver-{q.QueueName}"
            };

            return Client.CreateReceiver(q.QueueName, receiverOptions);
        }

        private ServiceBusClient CreateClient()
        {
            var clientOptions = new ServiceBusClientOptions
            {
                TransportType = _options.TransportType,
                RetryOptions = _options.Retry.ToServiceBusRetryOptions()
            };

            if (!string.IsNullOrWhiteSpace(_options.ConnectionString))
            {
                return new ServiceBusClient(_options.ConnectionString, clientOptions);
            }

            if (!string.IsNullOrWhiteSpace(_options.FullyQualifiedNamespace))
            {
                return new ServiceBusClient(
                    _options.FullyQualifiedNamespace,
                    new Azure.Identity.DefaultAzureCredential(),
                    clientOptions);
            }

            throw new InvalidOperationException(
                "Either ConnectionString or FullyQualifiedNamespace must be configured in AzureServiceBus settings.");
        }

        private QueueOptions GetQueueOptions(string logicalName)
        {
            if (!_options.Queues.TryGetValue(logicalName, out var q))
            {
                throw new InvalidOperationException(
                    $"Queue '{logicalName}' not found in AzureServiceBus configuration.");
            }
            return q;
        }

        private TopicOptions GetTopicOptions(string logicalName)
        {
            if (!_options.Topics.TryGetValue(logicalName, out var t))
            {
                throw new InvalidOperationException(
                    $"Topic '{logicalName}' not found in AzureServiceBus configuration.");
            }
            return t;
        }
        public ServiceBusReceiver GetOrCreateQueueReceiver(string logicalName)
        {
            var q = GetQueueOptions(logicalName);
            var cacheKey = $"queue-rx:{q.QueueName}:{q.SubQueue}";
            return _receivers.GetOrAdd(cacheKey, _ => CreateQueueReceiver(logicalName));            
        }
        public ServiceBusReceiver CreateSubscriptionReceiver(string topicLogicalName, string subscriptionLogicalName)
        {
            var topic = GetTopicOptions(topicLogicalName);
            if (!topic.Subscriptions.TryGetValue(subscriptionLogicalName, out var sub))
            {
                throw new InvalidOperationException(
                    $"Subscription '{subscriptionLogicalName}' not found under topic '{topicLogicalName}' in configuration.");
            }
            var receiverOptions = new ServiceBusReceiverOptions
            {
                ReceiveMode = sub.ReceiveMode,
                PrefetchCount = sub.PrefetchCount,
                SubQueue = sub.SubQueue,
                Identifier = $"receiver-{topic.TopicName}-{sub.SubscriptionName}"
            };
            return Client.CreateReceiver(topic.TopicName, sub.SubscriptionName, receiverOptions);
        }
        public ServiceBusReceiver GetOrCreateSubscriptionReceiver(string topicLogicalName, string subscriptionLogicalName)
        {
            var topic = GetTopicOptions(topicLogicalName);
            if (!topic.Subscriptions.TryGetValue(subscriptionLogicalName, out var sub))
            {
                throw new InvalidOperationException(
                    $"Subscription '{subscriptionLogicalName}' not found under topic '{topicLogicalName}' in configuration.");
            }
            var cacheKey = $"sub-rx:{topic.TopicName}:{sub.SubscriptionName}:{sub.SubQueue}";
            return _receivers.GetOrAdd(cacheKey, _ => CreateSubscriptionReceiver(topicLogicalName, subscriptionLogicalName));
        }
        public async ValueTask DisposeAsync()
        {
            foreach (var sender in _senders.Values)
            {
                await sender.DisposeAsync().ConfigureAwait(false);
            }

            foreach (var processor in _processors)
            {
                if (processor.IsProcessing)
                {
                    await processor.StopProcessingAsync().ConfigureAwait(false);
                }
                await processor.DisposeAsync().ConfigureAwait(false);
            }

            foreach (var processor in _sessionProcessors)
            {
                if (processor.IsProcessing)
                {
                    await processor.StopProcessingAsync().ConfigureAwait(false);
                }
                await processor.DisposeAsync().ConfigureAwait(false);
            }

            if (_client.IsValueCreated)
            {
                await _client.Value.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
