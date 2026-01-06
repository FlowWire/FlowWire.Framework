using System.ComponentModel;
using System.Linq.Expressions;

namespace FlowWire.Framework.Abstractions.Internal;

[EditorBrowsable(EditorBrowsableState.Never)]
public record ScheduleActivityFromExpression(Expression Expression) : WorkflowCommand;
