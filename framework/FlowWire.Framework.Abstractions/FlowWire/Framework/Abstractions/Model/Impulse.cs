namespace FlowWire.Framework.Abstractions.Model;

/// <summary>
/// The envelope for a signal/event targeting a specific Flow.
/// </summary>
public class Impulse
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FlowId { get; set; } = string.Empty;

    /// <summary>
    /// The name of the Flow Definition (e.g. "OrderFlow").
    /// </summary>
    public string FlowType { get; set; } = string.Empty;

    /// <summary>
    /// The name of the signal to dispatch (e.g. "Cancel").
    /// </summary>
    public string ImpulseName { get; set; } = string.Empty;

    /// <summary>
    /// The payload of the signal. 
    /// Note: This is polymorphic and serialized/deserialized by the framework.
    /// </summary>
    public object? Payload { get; set; }

    /// <summary>
    /// Context headers (TraceParent, RetryCount, etc.)
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = [];

    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Number of times this impulse has been delivered (for Retry Policy).
    /// </summary>
    public int DeliveryCount { get; set; }
}