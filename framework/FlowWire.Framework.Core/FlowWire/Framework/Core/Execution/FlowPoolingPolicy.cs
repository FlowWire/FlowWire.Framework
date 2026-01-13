using FlowWire.Framework.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace FlowWire.Framework.Core.Execution;

internal sealed class FlowPoolingPolicy(Type flowType, IServiceProvider serviceProvider) : IPooledObjectPolicy<IFlow>
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ObjectFactory _factory = ActivatorUtilities.CreateFactory(flowType, Type.EmptyTypes);

    public IFlow Create()
    {
        return (IFlow)_factory(_serviceProvider, arguments: null);
    }

    public bool Return(IFlow obj)
    {
        obj.Reset();

        // TODO: If the object is somehow "broken" or invalid, return false to discard it (letting GC collect it).
        return true;
    }
}