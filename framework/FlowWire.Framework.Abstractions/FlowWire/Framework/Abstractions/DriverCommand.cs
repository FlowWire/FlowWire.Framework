namespace FlowWire.Framework.Abstractions;

/// <summary>
/// A driver command that does not return a result (void).
/// </summary>
public record DriverCommand(string Id, object?[] Input) : Internal.ScheduleDrive(Id, Input)
{
    public DriverCommand(string Name, string Method, object?[] Input) : this($"{Name}.{Method}", Input) { }
}

/// <summary>
/// A strongly typed driver command that returns a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the driver command.</typeparam>
public record DriverCommand<TResult>(string Id, object?[] Input) : Internal.ScheduleDrive(Id, Input)
{
    public DriverCommand(string Name, string Method, object?[] Input) : this($"{Name}.{Method}", Input) { }
}
