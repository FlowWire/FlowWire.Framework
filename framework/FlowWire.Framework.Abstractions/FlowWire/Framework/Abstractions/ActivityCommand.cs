namespace FlowWire.Framework.Abstractions;

/// <summary>
/// A strongly typed activity command that returns a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the activity.</typeparam>
public record ActivityCommand<TResult>(string Name, object[] Input) : Internal.ScheduleActivity(Name, Input);
