using FlowWire.Framework.Abstractions.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace FlowWire.Framework.Core.Storage;

internal class BackendResolver(IServiceProvider services)
{
    public IStorage GetBackend(string name)
    {
        var backend = services.GetKeyedService<IStorage>(name);

        return backend switch
        {
            null => throw new InvalidOperationException(
                $"FlowWire Storage Error: No backend registered with the name '{name}'. " +
                $"Did you forget to call .AddStorageBackend<T>(\"{name}\") in Program.cs?"),
            _ => backend
        };
    }
}