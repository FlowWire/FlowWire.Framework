namespace FlowWire.Framework.Abstractions;

/// <summary>
/// Applied by the Source Generator to expose compile-time metadata 
/// to the runtime engine without expensive reflection scanning.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class GeneratedFlowConfigurationAttribute(Type stateType) : Attribute
{
    public Type StateType { get; } = stateType;
}
