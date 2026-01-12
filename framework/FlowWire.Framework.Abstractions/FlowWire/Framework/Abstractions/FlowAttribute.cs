namespace FlowWire.Framework.Abstractions;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class FlowAttribute : Attribute
{
    /// <summary>
    /// Defines the execution mode (Lifecycle Policy) for this Flow.
    /// Default is <see cref="FlowMode.Memory"/>.
    /// </summary>
    public FlowMode Mode { get; set; } = FlowMode.Memory;
}