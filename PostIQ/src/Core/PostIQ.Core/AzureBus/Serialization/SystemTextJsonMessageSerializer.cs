using PostIQ.Core.AzureBus.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PostIQ.Core.AzureBus.Serialization
{
    /// <summary>
    /// Default serializer using System.Text.Json with sensible defaults.
    /// </summary>
    public sealed class SystemTextJsonMessageSerializer : IMessageSerializer
    {
        private readonly JsonSerializerOptions _options;

        public SystemTextJsonMessageSerializer(JsonSerializerOptions? options = null)
        {
            _options = options ?? new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                PropertyNameCaseInsensitive = true
            };
        }

        public string ContentType => "application/json";

        public BinaryData Serialize<TMessage>(TMessage message) where TMessage : class
        {
            return BinaryData.FromObjectAsJson(message, _options);
        }

        public TMessage Deserialze<TMessage>(BinaryData data) where TMessage : class
        {
            return data.ToObjectFromJson<TMessage>(_options)
               ?? throw new InvalidOperationException($"Failed to deserialize message to {typeof(TMessage).Name}");
        }
    }
}
