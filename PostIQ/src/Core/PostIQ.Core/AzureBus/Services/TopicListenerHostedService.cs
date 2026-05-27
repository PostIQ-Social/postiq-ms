using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostIQ.Core.AzureBus.Abstraction;
using PostIQ.Core.AzureBus.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using static PostIQ.Core.AzureBus.Abstraction.IMessageHandler;

namespace PostIQ.Core.AzureBus.Services
{
    /// <summary>
    /// Background service that listens for messages on a topic subscription and dispatches
    /// them to the registered <see cref="IMessageHandler{TMessage}"/>.
    /// One instance is created per topic+subscription registration.
    /// </summary>
    public sealed class TopicListenerHostedService<TMessage> : BackgroundService
        where TMessage : class
    {
        private readonly ServiceBusClientFactory _factory;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMessageSerializer _serializer;
        private readonly ILogger<TopicListenerHostedService<TMessage>> _logger;
        private readonly string _topicLogicalName;
        private readonly string _subscriptionLogicalName;
        private readonly SubscriptionOptions _subOptions;

        public TopicListenerHostedService(
            ServiceBusClientFactory factory,
            IServiceScopeFactory scopeFactory,
            IMessageSerializer serializer,
            IOptions<ServiceBusOptions> options,
            ILogger<TopicListenerHostedService<TMessage>> logger,
            string topicLogicalName,
            string subscriptionLogicalName)
        {
            _factory = factory;
            _scopeFactory = scopeFactory;
            _serializer = serializer;
            _logger = logger;
            _topicLogicalName = topicLogicalName;
            _subscriptionLogicalName = subscriptionLogicalName;

            var topicOptions = options.Value.Topics.TryGetValue(topicLogicalName, out var t)
                ? t
                : throw new InvalidOperationException($"Topic '{topicLogicalName}' not found in configuration.");

            _subOptions = topicOptions.Subscriptions.TryGetValue(subscriptionLogicalName, out var s)
                ? s
                : throw new InvalidOperationException($"Subscription '{subscriptionLogicalName}' not found under topic '{topicLogicalName}'.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_subOptions.AutoStartProcessor)
            {
                _logger.LogInformation(
                    "Subscription '{Topic}/{Sub}' processor is set to manual start. Skipping.",
                    _topicLogicalName, _subscriptionLogicalName);
                return;
            }

            if (_subOptions.EnableSessions)
            {
                await RunSessionProcessorAsync(stoppingToken).ConfigureAwait(false);
            }
            else
            {
                await RunStandardProcessorAsync(stoppingToken).ConfigureAwait(false);
            }
        }

        private async Task RunStandardProcessorAsync(CancellationToken stoppingToken)
        {
            var processor = _factory.CreateSubscriptionProcessor(_topicLogicalName, _subscriptionLogicalName);

            processor.ProcessMessageAsync += async args =>
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();

                try
                {
                    var message = _serializer.Deserialze<TMessage>(args.Message.Body);
                    _logger.LogDebug(
                        "Processing message from '{Topic}/{Sub}', MessageId={MessageId}",
                        _topicLogicalName, _subscriptionLogicalName, args.Message.MessageId);

                    await handler.HandleAsync(message, args, stoppingToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error processing message from '{Topic}/{Sub}', MessageId={MessageId}",
                        _topicLogicalName, _subscriptionLogicalName, args.Message.MessageId);
                    throw;
                }
            };

            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception,
                    "Service Bus error on '{Topic}/{Sub}': Source={Source}",
                    _topicLogicalName, _subscriptionLogicalName, args.ErrorSource);
                return Task.CompletedTask;
            };

            _logger.LogInformation("Starting processor for '{Topic}/{Sub}'", _topicLogicalName, _subscriptionLogicalName);

            await processor.StartProcessingAsync(stoppingToken).ConfigureAwait(false);
            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
        }

        private async Task RunSessionProcessorAsync(CancellationToken stoppingToken)
        {
            var processor = _factory.CreateSubscriptionSessionProcessor(_topicLogicalName, _subscriptionLogicalName);

            processor.ProcessMessageAsync += async args =>
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<ISessionMessageHandler<TMessage>>();

                try
                {
                    var message = _serializer.Deserialze<TMessage>(args.Message.Body);
                    _logger.LogDebug(
                        "Processing session message from '{Topic}/{Sub}', SessionId={SessionId}, MessageId={MessageId}",
                        _topicLogicalName, _subscriptionLogicalName, args.SessionId, args.Message.MessageId);

                    await handler.HandleAsync(message, args, stoppingToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error processing session message from '{Topic}/{Sub}', MessageId={MessageId}",
                        _topicLogicalName, _subscriptionLogicalName, args.Message.MessageId);
                    throw;
                }
            };

            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception,
                    "Service Bus session error on '{Topic}/{Sub}': Source={Source}",
                    _topicLogicalName, _subscriptionLogicalName, args.ErrorSource);
                return Task.CompletedTask;
            };

            _logger.LogInformation("Starting session processor for '{Topic}/{Sub}'", _topicLogicalName, _subscriptionLogicalName);

            await processor.StartProcessingAsync(stoppingToken).ConfigureAwait(false);
            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
        }
    }
}
