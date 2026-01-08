using Microsoft.Extensions.DependencyInjection;

namespace FlowWire.Framework.Core.Configuration;

internal class FlowWireBuilder(IServiceCollection services) : IFlowWireBuilder
{
    public IServiceCollection Services { get; } = services;
}