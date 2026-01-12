namespace FlowWire.Framework.Analyzers.Generators;

internal record ActivityInterfaceModel(string Namespace, string InterfaceName, EquatableArray<ActivityMethodModel> Methods);
