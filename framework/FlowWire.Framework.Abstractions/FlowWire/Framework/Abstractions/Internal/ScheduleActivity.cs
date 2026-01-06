using System.ComponentModel;

namespace FlowWire.Framework.Abstractions.Internal;

[EditorBrowsable(EditorBrowsableState.Never)]
public record ScheduleActivity(string Name, object[] Input) : WorkflowCommand;
