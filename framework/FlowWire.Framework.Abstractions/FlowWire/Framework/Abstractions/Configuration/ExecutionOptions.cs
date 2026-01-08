namespace FlowWire.Framework.Abstractions.Configuration;

public class ExecutionOptions
{
    /// <summary>
    /// How long a node holds a lock on a Flow before it expires.
    /// If the node crashes, another node picks it up after this time.
    /// Default: 30 seconds.
    /// </summary>
    public TimeSpan LockTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// How long to keep a Flow in the database after it has Finished (Completed/Failed).
    /// Default: 7 days. (Set to TimeSpan.Zero to delete immediately).
    /// </summary>
    public TimeSpan FinishedFlowRetention { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// If true, the History of execution (Audit Log) is kept in the State.
    /// If false, only the current state is stored (Saves space).
    /// </summary>
    public bool EnableExecutionHistory { get; set; } = true;
}