using System.ComponentModel;
using System.Linq.Expressions;

namespace FlowWire.Framework.Abstractions.Internal;

[EditorBrowsable(EditorBrowsableState.Never)]
public record ScheduleOperationFromExpression(Expression Expression) : FlowCommand;
