using System.Linq.Expressions;

namespace FlowWire.Framework.Abstractions;

public struct DriverProxy<T>
{
    public readonly FlowCommand Run(Expression<Func<T, Task>> action)
    {

        return new Internal.ScheduleDriveFromExpression(action);
    }

    public readonly FlowCommand Run<TResult>(Expression<Func<T, Task<TResult>>> action)
    {
        return new Internal.ScheduleDriveFromExpression(action);
    }
}
