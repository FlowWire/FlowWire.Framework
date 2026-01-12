using System.Collections.Immutable;
using System.Text;
using FlowWire.Framework.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace FlowWire.Framework.Analyzers.Generators;

[Generator]
public class FlowGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var flows = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidate(node),
                transform: static (ctx, _) => GetFlowModel(ctx))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(flows, static (spc, source) => Execute(spc, source!));
    }

    private static bool IsCandidate(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax c && c.AttributeLists.Count > 0;
    }

    private static FlowModel? GetFlowModel(GeneratorSyntaxContext context)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol symbol || !IsFlow(symbol))
        {
            return null;
        }

        var flowMode = GetFlowMode(symbol);
        var isCircuit = flowMode == FlowMode.Circuit;

        var stateFields = ImmutableArray.CreateBuilder<StateFieldModel>();
        var linkedFields = ImmutableArray.CreateBuilder<LinkedFieldModel>();
        var contextProp = ScanMembers(symbol, isCircuit, stateFields, linkedFields);

        var impulses = ImmutableArray.CreateBuilder<ImpulseMethodModel>();
        var probes = ImmutableArray.CreateBuilder<ProbeMethodModel>();
        var flowMethod = ScanMethods(symbol, isCircuit, impulses, probes);

        return new FlowModel(
            symbol.ContainingNamespace.ToDisplayString(),
            symbol.Name,
            new EquatableArray<StateFieldModel>(stateFields.ToImmutable()),
            contextProp,
            flowMethod,
            new EquatableArray<LinkedFieldModel>(linkedFields.ToImmutable()),
            new EquatableArray<ImpulseMethodModel>(impulses.ToImmutable()),
            new EquatableArray<ProbeMethodModel>(probes.ToImmutable())
        );
    }

    private static FlowMode GetFlowMode(INamedTypeSymbol symbol)
    {
        var flowAttr = GetAttribute(symbol, "FlowWire.Framework.Abstractions.FlowAttribute");
        if (flowAttr is not null)
        {
            foreach (var arg in flowAttr.NamedArguments)
            {
                if (arg.Key == "Mode" && arg.Value.Value is int val)
                {
                    return (FlowMode)val;
                }
            }
        }
        return FlowMode.Memory;
    }

    private static string? ScanMembers(
        INamedTypeSymbol symbol,
        bool isCircuit,
        ImmutableArray<StateFieldModel>.Builder stateFields,
        ImmutableArray<LinkedFieldModel>.Builder linkedFields)
    {
        string? contextProp = null;

        // Scan members (Properties and Fields)
        foreach (var member in symbol.GetMembers())
        {
            if (member is IPropertySymbol prop)
            {
                if (IsLink(prop))
                {
                    if (IsFlowState(prop.Type))
                    {
                        stateFields.Add(new StateFieldModel(prop.Name, prop.Type.ToDisplayString()));
                    }
                    else if (IsContextType(prop.Type))
                    {
                        contextProp = prop.Name;
                    }
                    else if (isCircuit)
                    {
                        linkedFields.Add(new LinkedFieldModel(prop.Name, prop.Type.ToDisplayString()));
                    }
                }
                else if (IsFlowContext(prop))
                {
                    contextProp = prop.Name;
                }
            }
            else if (member is IFieldSymbol field)
            {
                if (IsLink(field))
                {
                     if (IsContextType(field.Type))
                     {
                         contextProp = field.Name;
                     }
                     else if (IsFlowState(field.Type))
                     {
                        stateFields.Add(new StateFieldModel(field.Name, field.Type.ToDisplayString()));
                     }
                     else if (isCircuit)
                     {
                        linkedFields.Add(new LinkedFieldModel(field.Name, field.Type.ToDisplayString()));
                     }
                }
            }
        }

        return contextProp;
    }

    private static string? ScanMethods(
        INamedTypeSymbol symbol,
        bool isCircuit,
        ImmutableArray<ImpulseMethodModel>.Builder impulses,
        ImmutableArray<ProbeMethodModel>.Builder probes)
    {
        string? flowMethod = null;

        foreach (var method in symbol.GetMembers().OfType<IMethodSymbol>())
        {
            if (isCircuit && IsWire(method))
            {
                flowMethod = method.Name;
            }

            if (TryGetImpulse(method, out var impulseAttr))
            {
                var signalName = impulseAttr!.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? method.Name;
                var parameters = method.Parameters.Select(p => p.Type.ToDisplayString()).ToImmutableArray();
                impulses.Add(new ImpulseMethodModel(method.Name, signalName, new EquatableArray<string>(parameters)));
            }

            if (TryGetProbe(method, out var probeAttr))
            {
                var queryName = probeAttr!.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? method.Name;
                var parameters = method.Parameters.Select(p => p.Type.ToDisplayString()).ToImmutableArray();
                probes.Add(new ProbeMethodModel(method.Name, queryName, new EquatableArray<string>(parameters)));
            }
        }

        return flowMethod;
    }

    private static bool IsContextType(ITypeSymbol type)
    {
        return type.Name == "IFlowContext" || 
               type.ToDisplayString() == "FlowWire.Framework.Abstractions.IFlowContext";
    }

    private static bool IsFlow(INamedTypeSymbol symbol)
    {
        return symbol.GetAttributes().Any(static a =>
            a.AttributeClass?.ToDisplayString() == "FlowWire.Framework.Abstractions.FlowAttribute");
    }

    private static bool IsLink(ISymbol symbol)
    {
        return symbol.GetAttributes().Any(static a =>
            a.AttributeClass?.ToDisplayString() == "FlowWire.Framework.Abstractions.LinkAttribute");
    }

    private static bool IsWire(ISymbol symbol)
    {
        return HasAttribute(symbol, "FlowWire.Framework.Abstractions.WireAttribute");
    }
    private static bool IsFlowState(ISymbol symbol)
    {
        return HasAttribute(symbol, "FlowWire.Framework.Abstractions.FlowStateAttribute");
    }
    private static bool IsFlowContext(ISymbol symbol)
    {
        return HasAttribute(symbol, "FlowWire.Framework.Abstractions.FlowContextAttribute");
    }

    private static bool TryGetImpulse(ISymbol symbol, out AttributeData? attributeData)
    {
        attributeData = GetAttribute(symbol, "FlowWire.Framework.Abstractions.ImpulseAttribute");
        return attributeData is not null;
    }

    private static bool TryGetProbe(ISymbol symbol, out AttributeData? attributeData)
    {
        attributeData = GetAttribute(symbol, "FlowWire.Framework.Abstractions.ProbeAttribute");
        return attributeData is not null;
    }

    private static bool HasAttribute(ISymbol symbol, string attributeName)
    {
        return symbol.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == attributeName);
    }

    private static AttributeData? GetAttribute(ISymbol symbol, string attributeName)
    {
        return symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == attributeName);
    }

    private static void Execute(SourceProductionContext context, FlowModel model)
    {
        var sb = new StringBuilder();

        var stateTypeString = GetStateTypeString(model);

        AppendFileHeader(sb, model);
        AppendClassDefinition(sb, model, stateTypeString);
        AppendSetState(sb, model);
        AppendGetState(sb, model);
        AppendSetContext(sb, model);
        AppendReset(sb, model);
        AppendExecuteMethod(sb, model);
        AppendDispatchSignal(sb, model);
        AppendDispatchQuery(sb, model);

        sb.AppendLine("}"); // End Class

        context.AddSource($"{model.ClassName}_Flow.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static string GetStateTypeString(FlowModel model)
    {
        if (model.StateFields.Count() == 1)
        {
            return model.StateFields.First().Type;
        }
        else if (model.StateFields.Count() > 1)
        {
            return $"{model.ClassName}_State";
        }
        
        return "object";
    }

    private static void AppendFileHeader(StringBuilder sb, FlowModel model)
    {
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("using FlowWire.Framework.Abstractions;");
        sb.AppendLine("using System.ComponentModel;"); // For EditorBrowsable
        
        if (!string.IsNullOrEmpty(model.Namespace) && model.Namespace != "<global namespace>")
        {
            sb.AppendLine($"namespace {model.Namespace};");
        }

        sb.AppendLine();
    }

    private static void AppendClassDefinition(StringBuilder sb, FlowModel model, string stateTypeString)
    {
        sb.AppendLine($"[GeneratedFlowConfiguration(typeof({stateTypeString}))]");
        sb.AppendLine($"partial class {model.ClassName} : IFlow");
        sb.AppendLine("{");

        // Define composite state class if needed
        if (model.StateFields.Count() > 1) 
        {
             sb.AppendLine($"    public class {model.ClassName}_State");
             sb.AppendLine("    {");
             foreach(var field in model.StateFields) 
             {
                 sb.AppendLine($"        public {field.Type} {field.Name} {{ get; set; }}");
             }
             sb.AppendLine("    }");
             sb.AppendLine();
        }
    }

    private static void AppendSetState(StringBuilder sb, FlowModel model)
    {
        sb.AppendLine("    void IFlow.SetState(object state)");
        sb.AppendLine("    {");
        if (model.StateFields.Count() == 1)
        {
            var f = model.StateFields.First();
            sb.AppendLine($"        this.{f.Name} = ({f.Type})state;");
        }
        else if (model.StateFields.Count() > 1)
        {
             var typeName = $"{model.ClassName}_State";
             sb.AppendLine($"        var s = ({typeName})state;");
             foreach(var f in model.StateFields)
             {
                 sb.AppendLine($"        this.{f.Name} = s.{f.Name};");
             }
        }
        sb.AppendLine("    }");
    }

    private static void AppendGetState(StringBuilder sb, FlowModel model)
    {
        sb.AppendLine("    object IFlow.GetState()");
        sb.AppendLine("    {");
        if (model.StateFields.Count() == 1)
        {
            sb.AppendLine($"        return this.{model.StateFields.First().Name};");
        }
        else if (model.StateFields.Count() > 1)
        {
             var typeName = $"{model.ClassName}_State";
             sb.AppendLine($"        return new {typeName}");
             sb.AppendLine("        {");
             foreach(var f in model.StateFields)
             {
                 sb.AppendLine($"            {f.Name} = this.{f.Name},");
             }
             sb.AppendLine("        };");
        }
        else
        {
            sb.AppendLine("        return new object(); // No State property defined");
        }
        sb.AppendLine("    }");
    }

    private static void AppendSetContext(StringBuilder sb, FlowModel model)
    {
        sb.AppendLine("    void IFlow.SetContext(IFlowContext context)");
        sb.AppendLine("    {");
        if (model.ContextProp != null)
        {
            sb.AppendLine($"        this.{model.ContextProp} = context;");
        }

        // Also handle "Just-in-Time" initialization of [Inject] clients here
        foreach (var field in model.InjectedFields)
        {
            sb.AppendLine($"        if (this.{field.Name} == null) this.{field.Name} = context.GetService<{field.Type}>();");
        }

        sb.AppendLine("    }");
    }

    private static void AppendReset(StringBuilder sb, FlowModel model)
    {
        sb.AppendLine("    void IFlow.Reset()");
        sb.AppendLine("    {");
        foreach(var f in model.StateFields)
        {
             sb.AppendLine($"        this.{f.Name} = new {f.Type}();");
        }
        if (model.ContextProp != null)
        {
            sb.AppendLine($"        this.{model.ContextProp} = default!;");
        }
        sb.AppendLine("    }");
    }

    private static void AppendExecuteMethod(StringBuilder sb, FlowModel model)
    {
        sb.AppendLine("    FlowCommand IFlow.Execute()");
        sb.AppendLine("    {");
        if (model.FlowMethod != null)
        {
            sb.AppendLine($"        return this.{model.FlowMethod}();");
        }
        else
        {
            sb.AppendLine("        return Command.Finish(); // No [Wire] method found");
        }
        sb.AppendLine("    }");
    }

    private static void AppendDispatchSignal(StringBuilder sb, FlowModel model)
    {
        sb.AppendLine("    void IFlow.DispatchSignal(string signalName, object? arg)");
        sb.AppendLine("    {");

        if (model.Signals.Any())
        {
            sb.AppendLine("        switch (signalName)");
            sb.AppendLine("        {");
            foreach (var signal in model.Signals)
            {
                sb.AppendLine($"            case \"{signal.SignalName}\":");
                
                if (!signal.Parameters.Any())
                {
                    sb.AppendLine($"                this.{signal.MethodName}();");
                }
                else if (signal.Parameters.Count() == 1)
                {
                    sb.AppendLine($"                this.{signal.MethodName}(({signal.Parameters[0]})arg!);");
                }
                else
                {
                    sb.AppendLine($"                var args = (object[])arg!;");
                    
                    var callArgs = new StringBuilder();
                    var index = 0;
                    foreach (var paramType in signal.Parameters)
                    {
                        if (index > 0)
                        {
                            callArgs.Append(", ");
                        }
                        callArgs.Append($"({paramType})args[{index}]");
                        index++;
                    }
                    sb.AppendLine($"                this.{signal.MethodName}({callArgs});");
                }
                sb.AppendLine("                break;");
            }
            sb.AppendLine("            default:");
            sb.AppendLine("                // Unknown signal - ignore or throw based on policy");
            sb.AppendLine("                break;");
            sb.AppendLine("        }");
        }

        sb.AppendLine("    }");
    }

    private static void AppendDispatchQuery(StringBuilder sb, FlowModel model)
    {
        sb.AppendLine("    object? IFlow.DispatchQuery(string queryName, object? arg)");
        sb.AppendLine("    {");

        if (model.Queries.Any())
        {
            sb.AppendLine("        switch (queryName)");
            sb.AppendLine("        {");
            foreach (var query in model.Queries)
            {
                sb.AppendLine($"            case \"{query.QueryName}\":");
                
                if (!query.Parameters.Any())
                {
                     sb.AppendLine($"                return this.{query.MethodName}();");
                }
                else if (query.Parameters.Count() == 1)
                {
                    sb.AppendLine($"                return this.{query.MethodName}(({query.Parameters[0]})arg!);");
                }
                else
                {
                    sb.AppendLine($"                var args = (object[])arg!;");

                    var callArgs = new StringBuilder();
                    var index = 0;
                    foreach (var paramType in query.Parameters)
                    {
                        if (index > 0)
                        {
                            callArgs.Append(", ");
                        }
                        callArgs.Append($"({paramType})args[{index}]");
                        index++;
                    }

                    sb.AppendLine($"                return this.{query.MethodName}({callArgs});");
                }
            }
            sb.AppendLine("            default:");
            sb.AppendLine("                return null;"); // Unknown query
            sb.AppendLine("        }");
        }
        else 
        {
            sb.AppendLine("        return null;");
        }
    }
}
