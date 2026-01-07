namespace FlowWire.Framework.Abstractions;

/// <summary>
/// A strongly typed driver command that returns a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the driver command.</typeparam>
public record DriverCommand<TResult>(string Name, object[] Input) : Internal.ScheduleDrive(Name, Input);
