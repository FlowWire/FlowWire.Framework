using System.ComponentModel;

namespace FlowWire.Framework.Abstractions.Internal;

[EditorBrowsable(EditorBrowsableState.Never)]
public record WaitForSignal(string Name, TimeSpan? Timeout) : WorkflowCommand;
