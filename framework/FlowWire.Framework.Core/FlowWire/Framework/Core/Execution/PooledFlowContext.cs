using FlowWire.Framework.Abstractions;
using FlowWire.Framework.Core.Helpers;

namespace FlowWire.Framework.Core.Execution;

internal sealed class PooledFlowContext(IServiceProvider services) : IFlowContext
{
    private readonly IServiceProvider _services = services;
    private readonly DeterministicRandom _random = new(0);

    public string FlowId { get; private set; } = string.Empty;
    public DateTimeOffset CurrentUtc { get; private set; }
    public int CurrentTick { get; private set; }
    public Random Random => _random;
    public bool IsShadowMode { get; private set; }

    /// <summary>
    /// SETUP: Called by FlowExecutor when starting a new Tick.
    /// Overwrites the state for the specific flow execution.
    /// </summary>
    public void Initialize(string flowId, DateTimeOffset now, int currentTick = 0)
    {
        FlowId = flowId;
        CurrentUtc = now;
        CurrentTick = currentTick;
        IsShadowMode = false;

        var seed = flowId.GetHashCode() ^ (int)now.Ticks;
        _random.Reset(seed);
    }

    /// <summary>
    /// CLEANUP: Called by the ObjectPool Policy when returning the object.
    /// Ensures no data leaks ("State Bleeding") between different flows.
    /// </summary>
    public void Reset()
    {
        FlowId = string.Empty;
        CurrentUtc = default;
        CurrentTick = 0;
        IsShadowMode = false;

        _random.Reset(0);
    }

    public T GetService<T>()
    {
        var service = _services.GetService(typeof(T));
        if (service is null)
        {
            ThrowServiceNotFound<T>();
        }
        return (T)service!;
    }

    private static void ThrowServiceNotFound<T>()
    {
        throw new InvalidOperationException($"Service {typeof(T).Name} not found.");
    }
}
