namespace FlowWire.Framework.Abstractions.Configuration;

public class SerializationOptions
{
    /// <summary>
    /// The format used for [Probe] results cached in Redis.
    /// </summary>
    public SerializerType ProbeSerializer { get; set; } = SerializerType.MemoryPack;

    /// <summary>
    /// The format used for persisting [FlowState] in the database.
    /// </summary>
    public SerializerType StateSerializer { get; set; } = SerializerType.MemoryPack;
}
