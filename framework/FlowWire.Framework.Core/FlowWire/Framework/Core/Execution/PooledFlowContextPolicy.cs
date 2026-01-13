using Microsoft.Extensions.ObjectPool;

namespace FlowWire.Framework.Core.Execution;

internal sealed class PooledFlowContextPolicy(IServiceProvider services) : IPooledObjectPolicy<PooledFlowContext>
{
    public PooledFlowContext Create()
    {
        return new(services);
    }

    public bool Return(PooledFlowContext obj)
    {
        obj.Reset();
        return true;
    }
}