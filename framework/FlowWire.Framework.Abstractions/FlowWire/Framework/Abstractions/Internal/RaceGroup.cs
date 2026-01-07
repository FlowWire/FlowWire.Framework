using System.ComponentModel;

namespace FlowWire.Framework.Abstractions.Internal;

[EditorBrowsable(EditorBrowsableState.Never)]
public record RaceGroup(FlowCommand[] Branches) : FlowCommand;
