namespace FlowWire.Framework.Abstractions;

internal interface IWorkflowContext
{
    /// <summary>
    /// The unique ID of this workflow instance.
    /// </summary>
    string WorkflowId { get; }

    /// <summary>
    /// Current deterministic time. 
    /// Constant during a single Flow() tick.
    /// </summary>
    DateTimeOffset CurrentUtc { get; }

    /// <summary>
    /// A deterministic random number generator seeded by (WorkflowID + Tick).
    /// </summary>
    Random Random { get; }

    /// <summary>
    /// The current version of the executed logic (useful for migration checks).
    /// </summary>
    int CurrentTick { get; }

    /// <summary>
    /// Check if we are currently replaying history
    /// or simply providing context on if this is a "Dry Run" (Shadow Mode).
    /// </summary>
    bool IsShadowMode { get; }
}
