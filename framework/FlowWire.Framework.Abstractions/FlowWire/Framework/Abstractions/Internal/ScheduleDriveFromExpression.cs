using System.ComponentModel;
using System.Linq.Expressions;

namespace FlowWire.Framework.Abstractions.Internal;

[EditorBrowsable(EditorBrowsableState.Never)]
public record ScheduleDriveFromExpression(Expression Expression) : FlowCommand;
