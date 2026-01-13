using MemoryPack;

namespace FlowWire.Framework.Abstractions.Model;

/// <summary>
/// The envelope for a signal/event targeting a specific Flow.
/// </summary>
[MemoryPackable(GenerateType.VersionTolerant)]
public partial class Impulse
{
    private string? _id;
    private Dictionary<string, string>? _headers;
    private DateTimeOffset? _receivedAt;

    [MemoryPackOrder(0)]
    public string Id
    {
        get => _id ??= Guid.NewGuid().ToString();
        set => _id = value;
    }

    [MemoryPackOrder(1)]
    public string FlowId { get; set; } = string.Empty;

    /// <summary>
    /// The name of the Flow Definition (e.g. "OrderFlow").
    /// </summary>
    [MemoryPackOrder(2)]
    public string FlowType { get; set; } = string.Empty;

    /// <summary>
    /// The name of the signal to dispatch (e.g. "Cancel").
    /// </summary>
    [MemoryPackOrder(3)]
    public string ImpulseName { get; set; } = string.Empty;

    /// <summary>
    /// The payload of the signal. 
    /// Note: This is polymorphic and serialized/deserialized by the framework.
    /// </summary>
    [MemoryPackIgnore]
    public object? Payload { get; set; }

    /// <summary>
    /// Context headers (TraceParent, RetryCount, etc.)
    /// </summary>
    [MemoryPackOrder(4)]
    public Dictionary<string, string> Headers 
    {
        get => _headers ??= [];
        set => _headers = value;
    }

    [MemoryPackOrder(5)]
    public DateTimeOffset ReceivedAt 
    {
        get => _receivedAt ??= DateTimeOffset.UtcNow;
        set => _receivedAt = value;
    }

    /// <summary>
    /// Number of times this impulse has been delivered (for Retry Policy).
    /// </summary>
    [MemoryPackOrder(6)]
    public int DeliveryCount { get; set; }
}