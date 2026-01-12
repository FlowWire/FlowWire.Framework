namespace FlowWire.Framework.Abstractions.Configuration;

/// <summary>
/// Configuration specific to the Orchestrator (The "Brain") background service.
/// </summary>
public class OrchestratorOptions
{
    /// <summary>
    /// The number of parallel consumers processing Impulse "Ticks".
    /// <br/>
    /// Default: Environment.ProcessorCount (CPU Bound work).
    /// </summary>
    public int Concurrency { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// The base polling interval when the queue is empty.
    /// <br/>
    /// Default: 50ms.
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// The maximum number of Inbox items (Signals) to fetch and process in a single Tick.
    /// <br/>
    /// Default: 100.
    /// </summary>
    public int MaxInboxBatchSize { get; set; } = 100;

    /// <summary>
    /// Grace period for the orchestrator to finish current Ticks during shutdown.
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
