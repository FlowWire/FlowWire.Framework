namespace FlowWire.Framework.Abstractions;

/// <summary>
/// Marks a field or property as a Linked Component.
/// <para>
/// The Framework will automatically wire up this connection before the Flow executes.
/// Use this to access [FlowState] and [Driver]
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class LinkAttribute : Attribute
{
}