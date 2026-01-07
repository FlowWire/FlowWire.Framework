namespace FlowWire.Framework.Abstractions;

/// <summary>
/// Marks a method as an Impulse Handler.
/// </summary>
/// <param name="name">Optional override for the impulse name.</param>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ImpulseAttribute(string? name = null) : Attribute
{
    /// <summary>
    /// The public name of the impulse (e.g., "Approve").
    /// If null, the method name is used.
    /// </summary>
    public string? Name { get; } = name;
}