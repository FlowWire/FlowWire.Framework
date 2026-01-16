using System.Collections.Frozen;
using System.Reflection;
using FlowWire.Framework.Abstractions;
using FlowWire.Framework.Core.Helpers;
using FlowWire.Framework.Core.Logging;
using Microsoft.Extensions.Logging;

namespace FlowWire.Framework.Core.Registry;

/// <summary>
/// A read-only, high-performance lookup for mapping Flow Type Names (strings) 
/// to their concrete CLR Types.
/// </summary>
public class FlowTypeRegistry
{
    private readonly FrozenDictionary<string, FlowMetadata> _nameMap;
    private readonly FrozenDictionary<Type, FlowMetadata> _typeMap;

    public FlowTypeRegistry(ILogger<FlowTypeRegistry> logger)
    {
        var assemblies = AssemblyDiscovery.FindFlowWireAssemblies();

        var nameBuilder = new Dictionary<string, FlowMetadata>(StringComparer.Ordinal);
        var typeBuilder = new Dictionary<Type, FlowMetadata>();

        foreach (var asm in assemblies)
        {
            foreach (var type in asm.GetTypes())
            {
                if (type.GetCustomAttribute<FlowAttribute>() is null)
                {
                    continue;
                }

                var configAttr = type.GetCustomAttribute<GeneratedFlowConfigurationAttribute>();

                var stateType = configAttr?.StateType ?? typeof(object);

                var flowAttrInstance = type.GetCustomAttribute<FlowAttribute>()!;
                var mode = flowAttrInstance.Mode;

                var energizeImpulses = new HashSet<string>(StringComparer.Ordinal);
                // Energize logic only applies to Circuits
                if (mode == FlowMode.Circuit)
                {
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var impulseAttr = method.GetCustomAttribute<ImpulseAttribute>();
                        if (impulseAttr != null && impulseAttr.Energizes)
                        {
                            var signalName = impulseAttr.Name ?? method.Name;
                            energizeImpulses.Add(signalName);
                        }
                    }
                }

                var meta = new FlowMetadata(type, stateType, mode, energizeImpulses);

                if (!nameBuilder.TryAdd(type.Name, meta))
                {
                    logger.LogFailedRegisterFlowTypeBySimpleName(type.Name);
                }

                if (type.FullName is not null && !nameBuilder.TryAdd(type.FullName, meta))
                {
                    logger.LogFailedRegisterFlowTypeByFullName(type.FullName);
                }

                typeBuilder.TryAdd(type, meta);
            }
        }

        _nameMap = nameBuilder.ToFrozenDictionary(StringComparer.Ordinal);
        _typeMap = typeBuilder.ToFrozenDictionary();
    }

    /// <summary>
    /// Looks up metadata by Flow Name (e.g., from an Impulse payload).
    /// </summary>
    public FlowMetadata? GetFlowMetadata(string flowTypeName)
    {
        return _nameMap.GetValueOrDefault(flowTypeName);
    }

    /// <summary>
    /// Looks up metadata by Flow Type (e.g., from DI or Generics).
    /// </summary>
    public FlowMetadata? GetFlowMetadata(Type flowType)
    {
        return _typeMap.GetValueOrDefault(flowType);
    }

    /// <summary>
    /// Convenience method to get the State Type directly.
    /// Returns null if the type is not a registered Flow.
    /// </summary>
    public Type? GetStateType(Type flowType)
    {
        return _typeMap.GetValueOrDefault(flowType)?.StateType;
    }

    /// <summary>
    /// Returns all registered flow types (used for Pool warming).
    /// </summary>
    public IEnumerable<Type> GetAllFlowTypes()
    {
        return _typeMap.Keys;
    }
}
