using FlowWire.Framework.Analyzers.Generators;

namespace FlowWire.Framework.Analyzers.Tests;

public class ActivityGeneratorTests
{
    [Fact]
    public void Generate_BasicActivity_CreatesClient()
    {
        var source = @"
using System.Threading.Tasks;
using FlowWire.Framework.Abstractions;

namespace TestNamespace
{
    [Activity]
    public interface IMyService
    {
        Task DoWork(string input);
        Task<int> Calculate(int a, int b);
    }
}";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new ActivityGenerator(), source);

        Assert.Empty(diagnostics);
        Assert.Single(generated);

        var generatedCode = generated[0];
        Assert.Contains("class IMyService_Client", generatedCode);
        Assert.Contains("public WorkflowCommand DoWork(string input)", generatedCode);
        Assert.Contains("return Command.Run(\"IMyService.DoWork\", input);", generatedCode);

        Assert.Contains("public ActivityCommand<int> Calculate(int a, int b)", generatedCode);
        Assert.Contains("return Command.Run<int>(\"IMyService.Calculate\", a, b);", generatedCode);
    }

    [Fact]
    public void Generate_WithInjectAttribute_ExcludesFromClientAndPayload()
    {
        var source = @"
using System.Threading.Tasks;
using FlowWire.Framework.Abstractions;

namespace TestNamespace
{
    public class InjectAttribute : System.Attribute {}

    [Activity]
    public interface IInjectedService
    {
        Task Process([Inject] string service, int data);
    }
}";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new ActivityGenerator(), source);

        Assert.Empty(diagnostics);
        Assert.Single(generated);

        var generatedCode = generated[0];
        // Ensure "service" is NOT in the method signature
        Assert.Contains("public WorkflowCommand Process(int data)", generatedCode);

        // Ensure "service" is NOT passed to Command.Run
        Assert.Contains("return Command.Run(\"IInjectedService.Process\", data);", generatedCode);
    }

    [Fact]
    public void Generate_WithMultipleInjects_ExcludesAll()
    {
        var source = @"
using System.Threading.Tasks;
using FlowWire.Framework.Abstractions;

namespace TestNamespace
{
    public class InjectAttribute : System.Attribute {}

    [Activity]
    public interface IMultiInject
    {
        Task Complex([Inject] string s1, int a, [Inject] object s2, string b);
    }
}";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new ActivityGenerator(), source);

        Assert.Empty(diagnostics);
        var generatedCode = generated[0];

        // Should only have 'a' and 'b'
        Assert.Contains("public WorkflowCommand Complex(int a, string b)", generatedCode);
        Assert.Contains("Command.Run(\"IMultiInject.Complex\", a, b)", generatedCode);
    }

    [Fact]
    public void Generate_HandlesGenericParameters()
    {
        var source = @"
using System.Collections.Generic;
using System.Threading.Tasks;
using FlowWire.Framework.Abstractions;

namespace TestNamespace
{
    [Activity]
    public interface IGenericService
    {
        Task ProcessList(List<string> items);
        Task<Dictionary<string, int>> ProcessMap(Dictionary<string, int> map);
    }
}";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new ActivityGenerator(), source);

        Assert.Empty(diagnostics);
        var generatedCode = generated[0];

        // Roslyn ToDisplayString defaults to full type names, so we expect System.Collections.Generic.List<string>
        Assert.Contains("public WorkflowCommand ProcessList(System.Collections.Generic.List<string> items)", generatedCode);
        Assert.Contains("public ActivityCommand<System.Collections.Generic.Dictionary<string, int>> ProcessMap(System.Collections.Generic.Dictionary<string, int> map)", generatedCode);
        Assert.Contains("Command.Run<System.Collections.Generic.Dictionary<string, int>>(\"IGenericService.ProcessMap\", map)", generatedCode);
    }

    [Fact]
    public void Generate_PreservesNamespace()
    {
        var source = @"
using System.Threading.Tasks;
using FlowWire.Framework.Abstractions;

namespace My.Complex.Namespace
{
    [Activity]
    public interface INamespacedService
    {
        Task Do();
    }
}";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new ActivityGenerator(), source);

        Assert.Empty(diagnostics);
        var generatedCode = generated[0];

        Assert.Contains("namespace My.Complex.Namespace;", generatedCode);
        Assert.Contains("class INamespacedService_Client", generatedCode);
    }

    [Fact]
    public void Generate_IgnoresOtherAttributes()
    {
        var source = @"
using System;
using System.Threading.Tasks;
using FlowWire.Framework.Abstractions;

namespace TestNamespace
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ValidationAttribute : Attribute {}

    [Activity]
    public interface IValidationService
    {
        Task Validate([Validation] string input);
    }
}";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new ActivityGenerator(), source);

        Assert.Empty(diagnostics);
        var generatedCode = generated[0];

        // Ensure "input" IS included despite the attribute (because it's not [Inject])
        Assert.Contains("public WorkflowCommand Validate(string input)", generatedCode);
        Assert.Contains("Command.Run(\"IValidationService.Validate\", input)", generatedCode);
    }
}
