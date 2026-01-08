using FlowWire.Framework.Abstractions;

namespace FlowWire.Framework.Core.Serialization;

public class CacheSerializationException(string message, Abstractions.SerializerType format, Type targetType, Exception? inner = null) : Exception(message, inner)
{
    public Abstractions.SerializerType Format { get; } = format;
    public Type TargetType { get; } = targetType;
}
