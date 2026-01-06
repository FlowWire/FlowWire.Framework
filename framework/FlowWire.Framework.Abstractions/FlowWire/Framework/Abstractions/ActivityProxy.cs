using System.Linq.Expressions;

namespace FlowWire.Framework.Abstractions;

public struct ActivityProxy<T>
{
    public readonly WorkflowCommand Run(Expression<Func<T, Task>> action)
    {

        return new Internal.ScheduleActivityFromExpression(action);
    }

    public readonly WorkflowCommand Run<TResult>(Expression<Func<T, Task<TResult>>> action)
    {
        return new Internal.ScheduleActivityFromExpression(action);
    }
}