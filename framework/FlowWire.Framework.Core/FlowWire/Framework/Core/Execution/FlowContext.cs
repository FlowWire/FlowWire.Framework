using FlowWire.Framework.Abstractions;

namespace FlowWire.Framework.Core.Execution;

public class FlowContext(string flowId, DateTimeOffset currentUtc, Random random, IServiceProvider services) : IFlowContext
{
    public string FlowId { get; } = flowId;

    public DateTimeOffset CurrentUtc { get; } = currentUtc;

    public Random Random { get; } = random;

    public int CurrentTick { get; } = 0;

    public bool IsShadowMode { get; } = false;

    public T GetService<T>()
    {
        var service = services.GetService(typeof(T))
            ?? throw new InvalidOperationException($"Service {typeof(T).Name} not found.");

        return (T)service;
    }
}