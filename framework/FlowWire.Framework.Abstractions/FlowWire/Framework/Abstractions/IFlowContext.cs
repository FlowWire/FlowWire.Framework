namespace FlowWire.Framework.Abstractions;

public interface IFlowContext
{
    /// <summary>
    /// The unique ID of this flow instance.
    /// </summary>
    string FlowId { get; }

    /// <summary>
    /// Current deterministic time. 
    /// Constant during a single FlowWire tick.
    /// </summary>
    DateTimeOffset CurrentUtc { get; }

    /// <summary>
    /// A deterministic random number generator seeded by (FlowId + Tick).
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

    /// <summary>
    /// Resolve a linked service or client from the execution context.
    /// </summary>
    T GetService<T>();
}
