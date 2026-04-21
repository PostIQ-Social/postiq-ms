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
    /// Background service that listens for messages on a specific queue and dispatches
    /// them to the registered <see cref="IMessageHandler{TMessage}"/>.
    /// One instance is created per queue registration.
    /// </summary>
    public sealed class QueueListenerHostedService<TMessage> : BackgroundService
        where TMessage : class
    {
        private readonly ServiceBusClientFactory _factory;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMessageSerializer _serializer;
        private readonly ILogger<QueueListenerHostedService<TMessage>> _logger;
        private readonly string _queueLogicalName;
        private readonly QueueOptions _queueOptions;

        public QueueListenerHostedService(
            ServiceBusClientFactory factory,
            IServiceScopeFactory scopeFactory,
            IMessageSerializer serializer,
            IOptions<ServiceBusOptions> options,
            ILogger<QueueListenerHostedService<TMessage>> logger,
            string queueLogicalName)
        {
            _factory = factory;
            _scopeFactory = scopeFactory;
            _serializer = serializer;
            _logger = logger;
            _queueLogicalName = queueLogicalName;

            if (!options.Value.Queues.TryGetValue(queueLogicalName, out var q))
            {
                throw new InvalidOperationException(
                    $"Queue '{queueLogicalName}' not found in configuration.");
            }
            _queueOptions = q;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_queueOptions.AutoStartProcessor)
            {
                _logger.LogInformation("Queue '{Queue}' processor is set to manual start. Skipping.",
                    _queueLogicalName);
                return;
            }

            if (_queueOptions.EnableSessions)
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
            var processor = _factory.CreateQueueProcessor(_queueLogicalName);

            processor.ProcessMessageAsync += async args =>
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();

                try
                {
                    var message = _serializer.Deserialze<TMessage>(args.Message.Body);
                    _logger.LogDebug(
                        "Processing message from queue '{Queue}', MessageId={MessageId}, DeliveryCount={Count}",
                        _queueLogicalName, args.Message.MessageId, args.Message.DeliveryCount);

                    await handler.HandleAsync(message, args, stoppingToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error processing message from queue '{Queue}', MessageId={MessageId}",
                        _queueLogicalName, args.Message.MessageId);
                    throw;
                }
            };

            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception,
                    "Service Bus error on queue '{Queue}': Source={Source}, Namespace={Ns}",
                    _queueLogicalName, args.ErrorSource, args.FullyQualifiedNamespace);
                return Task.CompletedTask;
            };

            _logger.LogInformation("Starting processor for queue '{Queue}'", _queueLogicalName);
            await processor.StartProcessingAsync(stoppingToken).ConfigureAwait(false);

            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
        }

        private async Task RunSessionProcessorAsync(CancellationToken stoppingToken)
        {
            var processor = _factory.CreateQueueSessionProcessor(_queueLogicalName);

            processor.ProcessMessageAsync += async args =>
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<ISessionMessageHandler<TMessage>>();

                try
                {
                    var message = _serializer.Deserialze<TMessage>(args.Message.Body);
                    _logger.LogDebug(
                        "Processing session message from queue '{Queue}', SessionId={SessionId}, MessageId={MessageId}",
                        _queueLogicalName, args.SessionId, args.Message.MessageId);

                    await handler.HandleAsync(message, args, stoppingToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error processing session message from queue '{Queue}', MessageId={MessageId}",
                        _queueLogicalName, args.Message.MessageId);
                    throw;
                }
            };

            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception,
                    "Service Bus session error on queue '{Queue}': Source={Source}",
                    _queueLogicalName, args.ErrorSource);
                return Task.CompletedTask;
            };

            _logger.LogInformation("Starting session processor for queue '{Queue}'", _queueLogicalName);
            await processor.StartProcessingAsync(stoppingToken).ConfigureAwait(false);

            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
        }
    }
}
