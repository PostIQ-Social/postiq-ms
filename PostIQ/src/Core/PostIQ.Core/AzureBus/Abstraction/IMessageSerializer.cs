using System;
using System.Collections.Generic;
using System.Text;

namespace PostIQ.Core.AzureBus.Abstraction
{
    /// <summary>
    /// Abstraction for message serialization. Replace the default System.Text.Json
    /// implementation with your own (e.g. Newtonsoft, MessagePack, Protobuf).
    /// </summary>
    public interface IMessageSerializer
    {
        BinaryData Serialize<TMessage>(TMessage message) where TMessage : class;

        TMessage Deserialze<TMessage>(BinaryData data) where TMessage : class;

        string ContentType { get; }
    }
}
