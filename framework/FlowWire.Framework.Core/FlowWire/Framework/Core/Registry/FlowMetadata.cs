using FlowWire.Framework.Abstractions;

namespace FlowWire.Framework.Core.Registry;

public sealed record class FlowMetadata(
    Type FlowType,
    Type StateType,
    FlowMode Mode,
    HashSet<string> EnergizeImpulses
);