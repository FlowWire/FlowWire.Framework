using FlowWire.Framework.Abstractions;

namespace FlowWire.Framework.Core.Serialization;

public class CacheSerializationException(string message, CacheFormat format, Type targetType, Exception? inner = null) : Exception(message, inner)
{
    public CacheFormat Format { get; } = format;
    public Type TargetType { get; } = targetType;
}
