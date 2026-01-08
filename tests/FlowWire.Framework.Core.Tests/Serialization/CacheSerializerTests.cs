using System.Buffers;
using FlowWire.Framework.Abstractions;
using FlowWire.Framework.Core.Serialization;
using MemoryPack;

namespace FlowWire.Framework.Core.Tests.Serialization;

[MemoryPackable]
public partial record TestData
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
}

[MemoryPackable]
public partial record ComplexTestData
{
    public Guid SessionId { get; set; }
    public List<string> Tags { get; set; } = [];
    public TestData? InnerData { get; set; }
    public Dictionary<string, int> Scores { get; set; } = [];
}

public class CacheSerializerTests
{
    public static TheoryData<Abstractions.SerializerType> AllFormats =>
    [
        Abstractions.SerializerType.MemoryPack,
        Abstractions.SerializerType.MemoryPackCompressed,
        Abstractions.SerializerType.Json
    ];

    [Theory]
    [MemberData(nameof(AllFormats))]
    public void Serialize_Deserialize_RoundTrip(Abstractions.SerializerType format)
    {
        var data = new TestData { Id = 1, Name = "Test", CreatedAt = DateTime.UtcNow };
        var bytes = Core.Serialization.CacheSerializer.Serialize(data, format);
        var result = Core.Serialization.CacheSerializer.Deserialize<TestData>(bytes);

        Assert.NotNull(result);
        Assert.Equal(data.Id, result.Id);
        Assert.Equal(data.Name, result.Name);

        // Handling precision diffs between formats if necessary, but DateTime equality usually works if exact
        // For Json, DateTime precision might be truncated depending on serializer settings, but default usually ISO8601
        if (format == Abstractions.SerializerType.Json)
        {
            Assert.Equal(data.CreatedAt, result.CreatedAt, TimeSpan.FromMilliseconds(1));
        }
        else
        {
            Assert.Equal(data.CreatedAt, result.CreatedAt);
        }
    }

    [Theory]
    [MemberData(nameof(AllFormats))]
    public void Serialize_Deserialize_Null_ReturnsNull(Abstractions.SerializerType format)
    {
        TestData? data = null;
        var bytes = Core.Serialization.CacheSerializer.Serialize<TestData?>(data, format);
        var result = Core.Serialization.CacheSerializer.Deserialize<TestData>(bytes);

        Assert.Null(result);
    }

    [Theory]
    [MemberData(nameof(AllFormats))]
    public void Serialize_Deserialize_ComplexObject(Abstractions.SerializerType format)
    {
        var data = new ComplexTestData
        {
            SessionId = Guid.NewGuid(),
            Tags = ["one", "two", "three"],
            InnerData = new TestData { Id = 99, Name = "Inner", CreatedAt = DateTime.UtcNow },
            Scores = new Dictionary<string, int> { { "A", 1 }, { "B", 2 } }
        };

        var bytes = Core.Serialization.CacheSerializer.Serialize(data, format);
        var result = Core.Serialization.CacheSerializer.Deserialize<ComplexTestData>(bytes);

        Assert.NotNull(result);
        Assert.Equal(data.SessionId, result.SessionId);
        Assert.Equal(data.Tags, result.Tags);
        Assert.Equal(data.Scores, result.Scores);
        Assert.NotNull(result.InnerData);
        Assert.Equal(data.InnerData.Id, result.InnerData.Id);
    }

    [Fact]
    public void Serialize_UsingBufferWriter_WorksCorrectly()
    {
        var data = new TestData { Id = 10, Name = "Buffer", CreatedAt = DateTime.UtcNow };
        var writer = new ArrayBufferWriter<byte>();

        Core.Serialization.CacheSerializer.Serialize(writer, data, Abstractions.SerializerType.MemoryPack);

        var bytes = writer.WrittenSpan;
        var result = Core.Serialization.CacheSerializer.Deserialize<TestData>(bytes);

        Assert.NotNull(result);
        Assert.Equal(data.Name, result.Name);
    }

    [Fact]
    public void Deserialize_EmptyData_ReturnsDefault()
    {
        var result = Core.Serialization.CacheSerializer.Deserialize<TestData>([]);
        Assert.Null(result);
    }

    [Fact]
    public void Deserialize_DataTooShort_ThrowsOrFails()
    {
        // 1 byte is just the tag, but payload is missing. 
        // Serializers might throw or return default depending on implementation.
        // MemoryPack throws IndexOutOfRange or similar if data is truncated.
        // Json might throw JsonException.

        // Case: Only tag provided, no payload.
        byte[] data = [(byte)Abstractions.SerializerType.MemoryPack];

        // MemoryPack generally expects valid data. 
        Assert.ThrowsAny<Exception>(() => Core.Serialization.CacheSerializer.Deserialize<TestData>(data));
    }

    [Fact]
    public void Deserialize_InvalidFormatTag_ThrowsInvalidOperation()
    {
        byte[] invalidData = [255, 1, 2, 3];
        var ex = Assert.Throws<InvalidOperationException>(() => Core.Serialization.CacheSerializer.Deserialize<TestData>(invalidData));
        Assert.Contains("Unknown cache format tag", ex.Message);
    }

    [Fact]
    public void Deserialize_CorruptedCompressedData_Throws()
    {
        var data = new TestData { Id = 1, Name = "Test" };
        var validBytes = Core.Serialization.CacheSerializer.Serialize(data, Abstractions.SerializerType.MemoryPackCompressed);

        // Corrupt the data (after the format tag)
        validBytes[^1] = (byte)~validBytes[^1];
        validBytes[^2] = (byte)~validBytes[^2];

        // BrotliDecompressor should fail
        Assert.ThrowsAny<Exception>(() => Core.Serialization.CacheSerializer.Deserialize<TestData>(validBytes));
    }

    [Fact]
    public void TryDeserialize_ValidData_ReturnsTrueAndValue()
    {
        var data = new TestData { Id = 123, Name = "Try" };
        var bytes = Core.Serialization.CacheSerializer.Serialize(data, Abstractions.SerializerType.Json);

        var success = Core.Serialization.CacheSerializer.TryDeserialize<TestData>(bytes, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(123, result.Id);
    }

    [Theory]
    [MemberData(nameof(AllFormats))]
    public void TryDeserialize_CorruptedData_ReturnsFalse(Abstractions.SerializerType format)
    {
        // Construct invalid data: valid tag but missing payload.
        // This ensures deserializers run out of data or hit invalid structure.
        var bytes = new byte[] { (byte)format };

        // This expects CacheSerializationException to be caught internally.
        var success = Core.Serialization.CacheSerializer.TryDeserialize<TestData>(bytes, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void Serialize_ThrowsCacheSerializationException_WhenSerializationFails()
    {
        // Serialize a type that causes failure.
        // Since we can't easily make built-in types fail, let's use a self-referencing object for JSON 
        // if supported, or a getter that throws.

        var failure = new ThrowingObject();

        // Using Json format as it executes getters during serialization
        var ex = Assert.Throws<CacheSerializationException>(() =>
            Core.Serialization.CacheSerializer.Serialize(failure, Abstractions.SerializerType.Json));

        Assert.Equal(Abstractions.SerializerType.Json, ex.Format);
        Assert.Equal(typeof(ThrowingObject), ex.TargetType);

        // The inner exception is the one thrown by the property getter (InvalidOperationException)
        // System.Text.Json propagates it directly or wrapped. 
        // In previous run it was InvalidOperationException direct.
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    public class ThrowingObject
    {
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0025 // Use expression body for property
        public string Name {
            get => throw new InvalidOperationException("Serialization sabotage");
        }
#pragma warning restore IDE0025 // Use expression body for property
#pragma warning restore CA1822 // Mark members as static
    }
}
