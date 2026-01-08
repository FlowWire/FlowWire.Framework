using Microsoft.Extensions.DependencyInjection;

namespace FlowWire.Framework.Core.Configuration;

public interface IFlowWireBuilder
{
    /// <summary>
    /// The underlying service collection. 
    /// Used to register dependencies into the DI container.
    /// </summary>
    IServiceCollection Services { get; }
}