namespace FlowWire.Framework.Abstractions;

/// <summary>
/// A strongly typed operation command that returns a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the operation.</typeparam>
public record OperationCommand<TResult>(string Name, object[] Input) : Internal.ScheduleOperation(Name, Input);
