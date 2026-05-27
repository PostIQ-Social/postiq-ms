using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.AzureBus.Abstraction
{
    public interface IMessageHandler
    {
        /// <summary>
        /// Pull message for background processing - background process based.
        /// Implement this interface to handle messages of type <typeparamref name="TMessage"/>.
        /// Register one handler per queue/subscription via DI.
        /// </summary>
        public interface IMessageHandler<in TMessage> where TMessage : class
        {
            /// <summary>
            /// Process a deserialized message.
            /// </summary>
            /// <param name="message">The deserialized message body.</param>
            /// <param name="args">Raw Service Bus processor event args for advanced scenarios 
            /// (abandon, defer, dead-letter, access message properties, etc.).</param>
            /// <param name="cancellationtoken">Cancellation token.</param>
            Task HandleAsync(TMessage message, ProcessMessageEventArgs args, CancellationToken cancellationToken = default);
        }

        /// <summary>
        /// Implement this interface to handle session-aware messages of type <typeparamref name="TMessage"/>.
        /// </summary>
        public interface ISessionMessageHandler<in TMessage> where TMessage : class
        {
            Task HandleAsync(TMessage message, ProcessSessionMessageEventArgs args, CancellationToken cancellationToken = default);
        }
    }
}
