namespace FlowWire.Framework.Analyzers.Generators;

internal record ActivityMethodModel(string Name, string ReturnType, string? InnerReturnType, EquatableArray<ActivityParameterModel> Parameters);
