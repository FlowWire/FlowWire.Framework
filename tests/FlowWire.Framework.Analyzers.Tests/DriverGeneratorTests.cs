using FlowWire.Framework.Analyzers.Generators;

namespace FlowWire.Framework.Analyzers.Tests;

public class DriverGeneratorTests
{
    [Fact]
    public void Generate_BasicDriver_CreatesClient()
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
        Assert.Contains("class IMyService_Driver", generatedCode);
        Assert.Contains("public FlowCommand DoWork(string input)", generatedCode);
        Assert.Contains("return _proxy.Drive(\"DoWork\", input);", generatedCode);

        Assert.Contains("public DriverCommand<int> Calculate(int a, int b)", generatedCode);
        Assert.Contains("return _proxy.Drive<int, int, int>(\"Calculate\", a, b);", generatedCode);
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
        Assert.Contains("_proxy.Drive<System.Collections.Generic.Dictionary<string, int>, System.Collections.Generic.Dictionary<string, int>>(\"ProcessMap\", map)", generatedCode);
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
        Assert.Contains("class INamespacedService_Driver", generatedCode);
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
        Assert.Contains("_proxy.Drive(\"Validate\", input)", generatedCode);
    }

    [Fact]
    public void Generate_HighParameterCount_UsesParamsFallback()
    {
        var source = @"
            using System.Threading.Tasks;
            using FlowWire.Framework.Abstractions;

            namespace TestNamespace
            {
                [Driver]
                public interface IManyParamsService
                {
                    // 9 parameters
                    Task<int> DoWithMany(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9);
                }
            }";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new DriverGenerator(), source);

        Assert.Empty(diagnostics);
        var generatedCode = generated[0];

        // Should NOT generate <int, int, int, ...> for the input parameters because it exceeds 8
        // Should still generate <int> for the return type
        Assert.Contains("public DriverCommand<int> DoWithMany(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9)", generatedCode);
        
        // Use regex or exact string match to verify we don't have the long generic list
        // Expected: _proxy.Drive<int>("DoWithMany", p1, p2, p3, p4, p5, p6, p7, p8, p9);
        Assert.Contains("_proxy.Drive<int>(\"DoWithMany\", p1, p2, p3, p4, p5, p6, p7, p8, p9)", generatedCode);
    }

    [Fact]
    public void Generate_ZeroParameters_Works()
    {
        var source = @"
            using System.Threading.Tasks;
            using FlowWire.Framework.Abstractions;

            namespace TestNamespace
            {
                [Driver]
                public interface IEmptyService
                {
                    Task DoNothing();
                    Task<string> GetString();
                }
            }";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new DriverGenerator(), source);

        Assert.Empty(diagnostics);
        var generatedCode = generated[0];

        Assert.Contains("public FlowCommand DoNothing()", generatedCode);
        Assert.Contains("_proxy.Drive(\"DoNothing\");", generatedCode);

        Assert.Contains("public DriverCommand<string> GetString()", generatedCode);
        // Zero input params, so it should use the generic overload that only takes TResult
        Assert.Contains("_proxy.Drive<string>(\"GetString\");", generatedCode);
    }

    [Fact]
    public void Generate_NullableParameters_PreservesTypes()
    {
        var source = @"
            using System.Threading.Tasks;
            using FlowWire.Framework.Abstractions;

            namespace TestNamespace
            {
                [Driver]
                public interface INullableService
                {
                    Task Process(int? val, string? text);
                }
            }";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new DriverGenerator(), source);

        Assert.Empty(diagnostics);
        var generatedCode = generated[0];

        Assert.Contains("public FlowCommand Process(int? val, string? text)", generatedCode);
        Assert.Contains("_proxy.Drive(\"Process\", val, text)", generatedCode);
    }
}
