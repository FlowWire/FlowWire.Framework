using System.ComponentModel;

namespace FlowWire.Framework.Abstractions.Internal;

[EditorBrowsable(EditorBrowsableState.Never)]
public record ScheduleDrive(string Id, object?[] Input) : FlowCommand;
