using System.ComponentModel;

namespace FlowWire.Framework.Abstractions.Internal;

[EditorBrowsable(EditorBrowsableState.Never)]
public record Complete(object? Output) : WorkflowCommand;
