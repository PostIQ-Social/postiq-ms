well-structured document with proper Markdown formatting, including the tables and code blocks.

A production-ready, fully configurable .NET 8 library that provides a clean abstraction over Azure Service Bus. 
It covers on-demand (pull-based) subscribing, session processing, dead-letter handling, scheduled delivery, health checks, and pluggable configuration and DI.

---

## Table of Contents

1. [Features](#features)
2. [Installation](#installation)
3. [Configuration](#configuration)
    - [Authentication](#authentication)
    - [Root Options](#root-options)
    - [Retry Options](#retry-options)
    - [Queue Options](#queue-options)
    - [Topic Options](#topic-options)
    - [Subscription Options](#subscription-options)
    - [Full Configuration Example](#full-configuration-example)
4. [Registration (Dependency Injection)](#registration-dependency-injection)
    - [Core Infrastructure](#1-core-infrastructure)
    - [Queue Handlers (Background)](#2-queue-handlers-background-processor)
    - [Session Queue Handlers](#3-session-queue-handlers)
    - [Topic Subscription Handlers (Background)](#4-topic-subscription-handlers-background-processor)
    - [Override Config in Code](#5-override-config-in-code)
5. [Publishing Messages](#publishing-messages)
    - [Send to Queue](#send-to-a-queue)
    - [Send to Topic](#send-to-a-topic)
    - [Batch Send](#batch-send)
    - [Custom Message Properties](#custom-message-properties)
    - [Scheduled Messages](#scheduled-messages)
    - [Cancel Scheduled Messages](#cancel-scheduled-messages)
6. [Consuming Messages — Background Processor](#consuming-messages--background-processor)
    - [Queue Handler](#queue-handler)
    - [Topic Subscription Handler](#topic-subscription-handler)
    - [Manual Completion and Dead-Lettering](#manual-completion-and-dead-lettering)
    - [Deferring Messages](#deferring-messages)
    - [Abandoning Messages](#abandoning-messages)
7. [Consuming Messages — Pull-Based (No Background Process)](#consuming-messages--pull-based-no-background-process)
    - [Receive from Queue](#receive-from-a-queue)
    - [Receive from Topic Subscription](#receive-from-a-topic-subscription)
    - [Batch Receive](#batch-receive)
    - [Peek Without Removing](#peek-without-removing)
8. [Session-Enabled Processing](#session-enabled-processing)
    - [Configuration](#session-configuration)
    - [Session Handler](#session-handler)
    - [Session State Management](#session-state-management)
9. [Dead-Letter Queue](#dead-letter-queue)
    - [Sending to Dead-Letter](#sending-messages-to-dead-letter)
    - [Reading from Dead-Letter](#reading-from-dead-letter-queue)
10. [Custom Serialization](#custom-serialization)
11. [Health Checks](#health-checks)
12. [Architecture](#architecture)
    - [Project Structure](#project-structure)
    - [Key Abstractions](#key-abstractions)
    - [Internal Services](#internal-services)
    - [Lifecycle and Disposal](#lifecycle-and-disposal)
13. [API Reference](#api-reference)
    - [IMessagePublisher](#imessagepublisher)
    - [IMessageSubscriber](#imessagesubscriber)
    - [IMessageHandler<TMessage>](#imessagehandlertmessage)
    - [ISessionMessageHandler<TMessage>](#isessionmessagehandlertmessage)
    - [IMessageSerializer](#imessageserializer)
    - [ServiceCollectionExtensions](#servicecollectionextensions)

---

## Features

* **Queue and Topic support** – send and receive on queues; publish/subscribe on topics with named subscriptions
* **Two consumption models** – background processor (push-based via `IHostedService`) and on-demand subscriber (pull-based)
* **Session-aware processing** – built-in support for session-enabled queues and subscriptions with session state management
* **Batch messaging** – automatic batch splitting when messages exceed Azure's size limits
* **Scheduled messages** – schedule messages for future delivery and cancel them by sequence number
* **Dead-letter handling** – programmatic dead-lettering with reason/description and dedicated DLQ readers
* **Custom message properties** – set correlation ID, subject, TTL, session ID, and arbitrary application properties
* **Health checks** – built-in ASP.NET Core health check for Service Bus connectivity
* **Pluggable serialization** – default `System.Text.Json` serializer, replaceable with any `IMessageSerializer` implementation
* **Full DI integration** – one-line registration with `IServiceCollection` extensions
* **Scoped handlers** – each message is processed in its own DI scope (safe for EF DbContext, scoped services, etc.)
* **Configuration-driven** – all settings live in `appsettings.json` using logical names; no Azure resource names in code
* **Thread-safe client factory** – senders, receivers, and processors are cached and reused; clean async disposal

---

## Installation

Add a project reference to `AzServiceBus.Core`:

```xml
<ProjectReference Include="path\to\AzServiceBus.Core\AzServiceBus.Core.csproj" />
The core library brings in these NuGet dependencies automatically:PackagePurposeAzure.Messaging.ServiceBusAzure Service Bus SDKAzure.IdentityDefaultAzureCredential for Managed IdentityMicrosoft.Extensions.Hosting.AbstractionsBackgroundService / IHostedServiceMicrosoft.Extensions.DependencyInjection.AbstractionsDI registrationMicrosoft.Extensions.Diagnostics.HealthChecksHealth check integrationSystem.Text.JsonDefault serializerConfigurationAll configuration is placed under the "AzureServiceBus" section in appsettings.json. The library uses logical names (dictionary keys) to reference queues, topics, and subscriptions throughout your code, so you never hard-code Azure resource names.AuthenticationThe library supports two authentication methods. Set one of the following:MethodPropertyWhen to UseConnection StringConnectionStringLocal development, integration testsManaged IdentityFullyQualifiedNamespaceProduction (Azure-hosted apps)ConnectionString takes precedence if both are set. When using FullyQualifiedNamespace, the library creates a DefaultAzureCredential which automatically works with Managed Identity, Azure CLI, Visual Studio credentials, and environment variables.JSON// Option 1: Connection string (dev / test)
"ConnectionString": "Endpoint=sb://my-namespace.servicebus.windows.net/;SharedAccessKeyName=...;SharedAccessKey=..."

// Option 2: Managed Identity (production)
"FullyQualifiedNamespace": "my-namespace.servicebus.windows.net"
Root OptionsPropertyTypeDefaultDescriptionConnectionStringstring?nullService Bus connection stringFullyQualifiedNamespacestring?nullNamespace FQDN for Managed IdentityTransportTypeenumAmqpTcpAmqpTcp (best performance) or AmqpWebSockets (firewall-friendly)EnableHealthChecksbooltrueRegister the built-in ASP.NET Core health checkHealthCheckTimeoutSecondsint10Health check probe timeoutRetryobjectsee belowGlobal retry policyQueuesDictionary{}Named queue configurations (key = logical name)TopicsDictionary{}Named topic configurations (key = logical name)Retry OptionsNested under "Retry":PropertyTypeDefaultDescriptionModeenumExponentialFixed or ExponentialMaxRetriesint3Maximum retry attempts before failingDelaySecondsdouble0.8Base delay between retriesMaxDelaySecondsdouble60Maximum backoff delay (applies to Exponential)TryTimeoutSecondsdouble60Timeout for a single operation attemptQueue OptionsEach entry under "Queues" uses a logical name as the key:PropertyTypeDefaultDescriptionQueueNamestring""The actual Azure Service Bus queue nameSenderIdentifierstring?auto-generatedDiagnostic identifier attached to the senderMaxConcurrentCallsint1Max parallel message processingAutoCompleteMessagesbooltrueAuto-complete after handler returns successfullyPrefetchCountint0Number of messages to prefetchReceiveModeenumPeekLockPeekLock (safe, requires completion) or ReceiveAndDeleteMaxWaitTimeSecondsdouble60Max time receiver waits for a messageMaxAutoLockRenewalDurationSecondsdouble300Duration for auto lock renewalSubQueueenumNoneNone, DeadLetter, or TransferDeadLetterEnableSessionsboolfalseEnable session-based processingMaxConcurrentSessionsint8Concurrent sessions (when sessions enabled)MaxConcurrentCallsPerSessionint1Concurrent calls within a sessionSessionIdleTimeoutSecondsdouble0Session idle timeout; 0 = no timeoutMaxMessagesBatchint10Max messages per batch receiveMaxDeliveryCountint10Max delivery count threshold for manual checksAutoStartProcessorbooltrueWhether background processor auto-starts on app startupTopic OptionsEach entry under "Topics":PropertyTypeDefaultDescriptionTopicNamestring""The actual Azure Service Bus topic nameSenderIdentifierstring?auto-generatedDiagnostic identifier for the senderSubscriptionsDictionary{}Named subscription configurationsSubscription OptionsNested under "Topics.<Name>.Subscriptions". Each entry uses a logical name as the key:PropertyTypeDefaultDescriptionSubscriptionNamestring""The actual Azure subscription nameMaxConcurrentCallsint1Max parallel message processingAutoCompleteMessagesbooltrueAuto-complete after handler returnsPrefetchCountint0Messages to prefetchReceiveModeenumPeekLockPeekLock or ReceiveAndDeleteMaxAutoLockRenewalDurationSecondsdouble300Lock renewal durationSubQueueenumNoneNone, DeadLetter, TransferDeadLetterEnableSessionsboolfalseEnable session processingMaxConcurrentSessionsint8Concurrent sessionsMaxConcurrentCallsPerSessionint1Calls per sessionSessionIdleTimeoutSecondsdouble0Session idle timeoutMaxMessagesBatchint10Batch receive sizeAutoStartProcessorbooltrueAuto-start processor on startupFull Configuration ExampleJSON{
  "AzureServiceBus": {
    "ConnectionString": "Endpoint=sb://my-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR_KEY",
    "TransportType": "AmqpTcp",
    
    "Retry": {
      "Mode": "Exponential",
      "MaxRetries": 3,
      "DelaySeconds": 0.8,
      "MaxDelaySeconds": 60,
      "TryTimeoutSeconds": 60
    },

    "EnableHealthChecks": true,
    "HealthCheckTimeoutSeconds": 10,

    "Queues": {
      "Orders": {
        "QueueName": "orders-queue",
        "SenderIdentifier": "order-service-sender",
        "MaxConcurrentCalls": 5,
        "AutoCompleteMessages": true,
        "PrefetchCount": 10,
        "ReceiveMode": "PeekLock",
        "MaxWaitTimeSeconds": 60,
        "MaxAutoLockRenewalDurationSeconds": 300,
        "AutoStartProcessor": true
      },
      "Payments": {
        "QueueName": "payments-queue",
        "MaxConcurrentCalls": 3,
        "AutoCompleteMessages": false,
        "ReceiveMode": "PeekLock",
        "MaxAutoLockRenewalDurationSeconds": 600,
        "AutoStartProcessor": true
      },
      "OrdersDLQ": {
        "QueueName": "orders-queue",
        "SubQueue": "DeadLetter",
        "MaxConcurrentCalls": 1,
        "AutoCompleteMessages": false,
        "AutoStartProcessor": true
      },
      "SessionOrders": {
        "QueueName": "session-orders-queue",
        "EnableSessions": true,
        "MaxConcurrentSessions": 4,
        "MaxConcurrentCallsPerSession": 1,
        "AutoCompleteMessages": true,
        "AutoStartProcessor": true
      }
    },

    "Topics": {
      "Notifications": {
        "TopicName": "notifications-topic",
        "SenderIdentifier": "notification-publisher",
        "Subscriptions": {
          "EmailHandler": {
            "SubscriptionName": "email-subscription",
            "MaxConcurrentCalls": 3,
            "AutoCompleteMessages": true,
            "PrefetchCount": 5,
            "AutoStartProcessor": true
          },
          "SmsHandler": {
            "SubscriptionName": "sms-subscription",
            "MaxConcurrentCalls": 2,
            "AutoCompleteMessages": true,
            "AutoStartProcessor": true
          }
        }
      }
    }
  }
}
Registration (Dependency Injection)All registration is done through extension methods in AzServiceBus.Core.Extensions.ServiceCollectionExtensions.

1. Core Infrastructure
// Register from appsettings.json
services.AddAzServiceBus(builder.Configuration.GetSection("AzureServiceBus"));

// OR: Register manually (often for testing)
services.AddAzServiceBus(options => {
    options.ConnectionString = "...";
    options.Queues.Add("MyQueue", new QueueOptions { QueueName = "actual-queue" });
});

// Register the publishing/subscribing client
services.AddAzServiceBusClient();

2. Queue Handlers (Background Processor)
// Register a handler for the "Orders" logical queue
services.AddQueueHandler<OrderMessageHandler, OrderMessage>("Orders");

// Register a handler for the "Payments" logical queue
services.AddQueueHandler<PaymentMessageHandler, PaymentMessage>("Payments");

3. Session Queue Handlers
// Register a session-aware handler
services.AddSessionQueueHandler<OrderSessionHandler, OrderMessage>("SessionOrders");

4. Topic Subscription Handlers (Background Processor)
// Register a handler for the "EmailHandler" subscription under "Notifications" topic
services.AddTopicSubscriptionHandler<EmailNotificationHandler, NotificationMessage>("Notifications", "EmailHandler");

5. Override Config in Code
services.ConfigureAzServiceBus(options => {
    // You can modify configuration at runtime here
    if (options.Queues.TryGetValue("Orders", out var orderOptions)) {
        orderOptions.MaxConcurrentCalls = 10;
    }
});

Publishing MessagesInject IMessagePublisher to send messages.Send to a Queue
public class OrderService(IMessagePublisher publisher) {
    public async Task PlaceOrder(Order order) {
        // Send using logical queue name
        await publisher.SendToQueueAsync("Orders", order);
    }
}

Send to a Topic
await publisher.SendToTopicAsync("Notifications", new NotificationMessage { ... });

Batch Send
var messages = Enumerable.Range(0, 100).Select(i => new MyMessage { Id = i });

// Automatically splits into multiple Service Bus batches if the list exceeds the size limit
await publisher.SendBatchToQueueAsync("MyQueue", messages);

Custom Message Properties
await publisher.SendToQueueAsync("Orders", order, properties => {
    properties.CorrelationId = "my-corr-id";
    properties.Subject = "OrderCreated";
    properties.TimeToLive = TimeSpan.FromHours(1);
    properties.ApplicationProperties.Add("Region", "US-East");
});

Scheduled Messages
// Schedule for 5 minutes from now
long sequenceNumber = await publisher.ScheduleQueueMessageAsync("Orders", order, DateTimeOffset.UtcNow.AddMinutes(5));

Cancel Scheduled Messages
await publisher.CancelScheduledQueueMessageAsync("Orders", sequenceNumber);

Consuming Messages — Background ProcessorTo process messages automatically in the background, implement IMessageHandler<T>.Queue Handler
public class OrderMessageHandler : IMessageHandler<OrderMessage> {
    public async Task HandleAsync(OrderMessage message, MessageContext context, CancellationToken ct) {
        Console.WriteLine($"Processing Order: {message.OrderId}");
        // context provides access to headers, sequence number, etc.
    }

    public Task HandleExceptionAsync(Exception ex, string context) {
        // Log errors
        return Task.CompletedTask;
    }
}

Topic Subscription Handler
public class EmailNotificationHandler : IMessageHandler<NotificationMessage> {
    public async Task HandleAsync(NotificationMessage message, MessageContext context, CancellationToken ct) {
        // Handle notification...
    }

    public Task HandleExceptionAsync(Exception ex, string context) {
        return Task.CompletedTask;
    }
}

Manual Completion and Dead-LetteringIf AutoCompleteMessages is set to false, you must use the IMessageSubscriber inside your handler.
public async Task HandleAsync(OrderMessage message, MessageContext context, CancellationToken ct) {
    try {
        await _db.SaveAsync(message);
        // Manually complete
        await _subscriber.CompleteMessageAsync(context, ct);
    } catch (Exception ex) {
        // Manually dead-letter
        await _subscriber.DeadLetterMessageAsync(context, "DatabaseError", ex.Message, ct);
    }
}

Consuming Messages — Pull-BasedInject IMessageSubscriber for manual message retrieval.Receive from a Queue
var message = await subscriber.ReceiveAsync<OrderMessage>("Orders", cancellationToken);
if (message != null) {
    // Process...
    await subscriber.CompleteMessageAsync(message.Context);
}

Batch Receive
var messages = await subscriber.ReceiveBatchAsync<OrderMessage>("Orders", maxMessages: 10);

Session-Enabled ProcessingSession HandlerImplement ISessionMessageHandler<T>. This ensures all messages with the same SessionId are processed in order by the same handler instance.
public class OrderSessionHandler : ISessionMessageHandler<OrderMessage> {
    public async Task HandleAsync(OrderMessage message, SessionContext context, CancellationToken ct) {
        // context.SessionId is available here
    }
}

Session State Management
// Inside handler: Get state
var state = await context.GetStateAsync<MySessionState>();

// Update and set state
state.LastStep = "Processed";
await context.SetStateAsync(state);
Dead-Letter QueueReading from Dead-Letter QueueTo read messages from the DLQ, define a logical queue in config with SubQueue: "DeadLetter", then use a handler or subscriber as usual.
JSON"OrdersDLQ": {
  "QueueName": "orders-queue",
  "SubQueue": "DeadLetter"
}

services.AddQueueHandler<DlqHandler, OrderMessage>("OrdersDLQ");

Health ChecksThe library automatically registers a health check named "AzServiceBus". You can map it in your Program.cs:
app.MapHealthChecks("/health");
ArchitectureScoped Processing: Every message handled by a background processor creates a new IServiceScope. This ensures that scoped dependencies (like Entity Framework DbContext) are disposed of correctly after each message.Resilience: Built-in retry policies apply to all operations.Abstraction: The library wraps ServiceBusSender, ServiceBusProcessor, and ServiceBusReceiver to handle the complexity of connection management and logical-to-physical name mapping.