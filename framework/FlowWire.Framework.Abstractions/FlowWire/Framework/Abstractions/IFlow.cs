using System.ComponentModel;

namespace FlowWire.Framework.Abstractions;

/// <summary>
/// The non-generic contract used by the Orchestrator to drive user flows.
/// Users should not implementing this directly, instead they should use [Flow].
/// The source generator will create implementations of this interface behind the scenes.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IFlow
{
    /// <summary>
    /// Hydrates the brain with state from the database.
    /// </summary>
    void SetState(object state);

    /// <summary>
    /// Injects the deterministic sandbox (Time, Random, FlowID).
    /// </summary>
    void SetContext(IFlowContext context);

    /// <summary>
    /// Extracts the current state to save back to the database.
    /// </summary>
    object GetState();

    /// <summary>
    /// Routes an incoming signal to the correct method.
    /// This is typically implemented via Source Generators to avoid Reflection.
    /// </summary>
    void DispatchSignal(string signalName, object? arg);

    /// <summary>
    /// Dispatches a query to the appropriate handler.
    /// </summary>
    object? DispatchQuery(string queryName, object? arg);

    /// <summary>
    /// Executes the user's logic loop (Decide).
    /// </summary>
    FlowCommand Execute();

    /// <summary>
    /// Clears references to state/context.
    /// Critical for Object Pooling to prevent memory leaks.
    /// </summary>
    void Reset();
}