namespace FlowWire.Framework.Analyzers.Generators;

internal record FlowModel(
    string Namespace,
    string ClassName,
    EquatableArray<StateFieldModel> StateFields,
    string? ContextProp,
    string? FlowMethod,
    EquatableArray<LinkedFieldModel> InjectedFields,
    EquatableArray<ImpulseMethodModel> Signals,
    EquatableArray<ProbeMethodModel> Queries
);
