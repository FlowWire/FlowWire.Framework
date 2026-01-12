namespace FlowWire.Framework.Abstractions;

/// <summary>
/// Defines the lifecycle and creation policy of a Flow.
/// </summary>
public enum FlowMode
{
    /// <summary>
    /// <b>(Default)</b> The Flow acts as a <b>Memory</b>.
    /// <br/>
    /// <b>Metaphor:</b> A passive memory bank.
    /// <br/>
    /// <b>Behavior:</b>
    /// <list type="bullet">
    /// <item>It is purely "Reactive" (Loose). Any Impulse will upsert the state.</item>
    /// <item>It implies the Flow is primarily a State Container (Entity).</item>
    /// <item>Perfect for Digital Twins, Shopping Carts, and IoT Shadows.</item>
    /// </list>
    /// </summary>
    Memory = 0,

    /// <summary>
    /// The Flow acts as a <b>Circuit</b>.
    /// <br/>
    /// <b>Metaphor:</b> An active logical network.
    /// <br/>
    /// <b>Behavior:</b>
    /// <list type="bullet">
    /// <item>It is "Gated" (Strict). It requires a specific startup signal (<c>IsInit</c>) to energize.</item>
    /// <item>It implies the Flow has a complex Lifecycle or Process.</item>
    /// <item>Perfect for Orders, Payments, and Workflows.</item>
    /// </list>
    /// </summary>
    Circuit = 1
}
