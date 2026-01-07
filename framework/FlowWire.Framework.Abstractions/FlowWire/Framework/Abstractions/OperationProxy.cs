using System.Linq.Expressions;

namespace FlowWire.Framework.Abstractions;

public struct OperationProxy<T>
{
    public readonly FlowCommand Run(Expression<Func<T, Task>> action)
    {

        return new Internal.ScheduleOperationFromExpression(action);
    }

    public readonly FlowCommand Run<TResult>(Expression<Func<T, Task<TResult>>> action)
    {
        return new Internal.ScheduleOperationFromExpression(action);
    }
}