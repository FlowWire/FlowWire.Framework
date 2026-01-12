namespace FlowWire.Framework.Abstractions.Configuration;

public class FlowWireOptions
{
    /// <summary>
    /// Options for connecting to external services and infrastructure.
    /// </summary>
    public ConnectionOptions Connection { get; } = new();

    // 2. Execution & Orchestration (The Brain)
    /// <summary>
    /// Options for the execution and orchestration.
    /// </summary>
    public ExecutionOptions Execution { get; } = new();

    /// <summary>
    /// Options for state management and data storage.
    /// </summary>
    public StorageOptions Storage { get; } = new();

    /// <summary>
    /// Settings for the Orchestrator service (Logic execution).
    /// </summary>
    public OrchestratorOptions Orchestrator { get; set; } = new();

    /// <summary>
    /// Options for worker processes and driver execution.
    /// </summary>
    public WorkerOptions Worker { get; } = new();

    /// <summary>
    /// Options for serialization and data formats.
    /// </summary>
    public SerializationOptions Serialization { get; } = new();
}