namespace FlowWire.Framework.Abstractions;

/// <summary>
/// Marks a method as a Signal Handler.
/// </summary>
/// <param name="name">Optional override for the signal name.</param>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class SignalAttribute(string? name = null) : Attribute
{
    /// <summary>
    /// The public name of the signal (e.g., "Approve").
    /// If null, the method name is used.
    /// </summary>
    public string? Name { get; } = name;
}