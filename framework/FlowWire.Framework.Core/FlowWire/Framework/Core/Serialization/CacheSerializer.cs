using System.Buffers;
using System.Text.Json;
using FlowWire.Framework.Abstractions;
using MemoryPack;
using MemoryPack.Compression;

namespace FlowWire.Framework.Core.Serialization;

public static class CacheSerializer
{
    public static byte[] Serialize<T>(T value, Abstractions.SerializerType format)
    {
        var writer = new ArrayBufferWriter<byte>();
        Serialize(writer, value, format);
        return writer.WrittenSpan.ToArray();
    }

    public static void Serialize<T>(IBufferWriter<byte> writer, T value, Abstractions.SerializerType format)
    {
        if (value is null)
        {
            return;
        }

        SetCacheFormatTag(ref writer, format);
        
        try
        {
            switch (format)
            {
                case Abstractions.SerializerType.MemoryPack:
                    MemoryPackSerializer.Serialize(writer, value);
                    break;

                case Abstractions.SerializerType.MemoryPackCompressed:
                    using (var compressor = new BrotliCompressor())
                    {
                        MemoryPackSerializer.Serialize(compressor, value);
                        compressor.CopyTo(writer);
                    }
                    break;

                case Abstractions.SerializerType.Json:
                    using (var jsonWriter = new Utf8JsonWriter(writer))
                    {
                        JsonSerializer.Serialize(jsonWriter, value);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported cache format.");
            }
        }
        catch (Exception ex) when (ex is not ArgumentOutOfRangeException)
        {
            throw new CacheSerializationException(
                $"Failed to serialize {typeof(T).Name} using {format}.",
                format,
                typeof(T),
                ex);
        }
    }

    public static T? Deserialize<T>(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
        {
            return default;
        }

        var cacheFormatTag = (Abstractions.SerializerType)data[0];
        var payload = data[1..];

        try
        {
            return cacheFormatTag switch
            {
                Abstractions.SerializerType.MemoryPack => MemoryPackSerializer.Deserialize<T>(payload),
                Abstractions.SerializerType.MemoryPackCompressed => DeserializeCompressed<T>(payload),
                Abstractions.SerializerType.Json => JsonSerializer.Deserialize<T>(payload),
                _ => throw new InvalidOperationException($"Unknown cache format tag: {cacheFormatTag}")
            };
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new CacheSerializationException(
                $"Failed to deserialize {typeof(T).Name} using {cacheFormatTag}.",
                cacheFormatTag,
                typeof(T),
                ex);
        }
    }

    public static bool TryDeserialize<T>(ReadOnlySpan<byte> data, out T? value)
    {
        try
        {
            value = Deserialize<T>(data);
            return true;
        }
        catch (CacheSerializationException)
        {
            value = default;
            return false;
        }
    }

    private static T? DeserializeCompressed<T>(ReadOnlySpan<byte> compressed)
    {
        using var decompressor = new BrotliDecompressor();
        var decompressed = decompressor.Decompress(compressed);
        return MemoryPackSerializer.Deserialize<T>(decompressed);
    }

    private static void SetCacheFormatTag(ref IBufferWriter<byte> writer, Abstractions.SerializerType format)
    {
        var span = writer.GetSpan(1);
        span[0] = (byte)format;
        writer.Advance(1);
    }
}