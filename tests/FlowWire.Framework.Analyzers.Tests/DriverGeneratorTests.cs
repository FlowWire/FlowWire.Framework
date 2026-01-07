using FlowWire.Framework.Analyzers.Generators;
using Xunit;

namespace FlowWire.Framework.Analyzers.Tests;

public class DriverGeneratorTests
{
    [Fact]
    public void Generate_BasicOp_CreatesClient()
    {
        var source = @"
            using System.Threading.Tasks;
            using FlowWire.Framework.Abstractions;

            namespace TestNamespace
            {
                [Driver]
                public interface IMyService
                {
                    Task DoWork(string input);
                    Task<int> Calculate(int a, int b);
                }
            }";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new DriverGenerator(), source);

        Assert.Empty(diagnostics);
        Assert.Single(generated);

        var generatedCode = generated[0];
        Assert.Contains("class IMyService_Client", generatedCode);
        Assert.Contains("public FlowCommand DoWork(string input)", generatedCode);
        Assert.Contains("return Command.Run(\"IMyService.DoWork\", input);", generatedCode);

        Assert.Contains("public DriverCommand<int> Calculate(int a, int b)", generatedCode);
        Assert.Contains("return Command.Run<int>(\"IMyService.Calculate\", a, b);", generatedCode);
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
                [Driver]
                public interface IGenericService
                {
                    Task ProcessList(List<string> items);
                    Task<Dictionary<string, int>> ProcessMap(Dictionary<string, int> map);
                }
            }";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new DriverGenerator(), source);

        Assert.Empty(diagnostics);
        var generatedCode = generated[0];

        // Roslyn ToDisplayString defaults to full type names, so we expect System.Collections.Generic.List<string>
        Assert.Contains("public FlowCommand ProcessList(System.Collections.Generic.List<string> items)", generatedCode);
        Assert.Contains("public DriverCommand<System.Collections.Generic.Dictionary<string, int>> ProcessMap(System.Collections.Generic.Dictionary<string, int> map)", generatedCode);
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
                [Driver]
                public interface INamespacedService
                {
                    Task Do();
                }
            }";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new DriverGenerator(), source);

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

                [Driver]
                public interface IValidationService
                {
                    Task Validate([Validation] string input);
                }
            }";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new DriverGenerator(), source);

        Assert.Empty(diagnostics);
        var generatedCode = generated[0];

        // Ensure "input" IS included despite the attribute
        Assert.Contains("public FlowCommand Validate(string input)", generatedCode);
        Assert.Contains("Command.Run(\"IValidationService.Validate\", input)", generatedCode);
    }
}
