using FlowWire.Framework.Analyzers.Generators;

namespace FlowWire.Framework.Analyzers.Tests;

public class FlowGeneratorTests
{
    [Fact]
    public void Generate_BasicFlow_ImplementsIFlow()
    {
        var source = @"
            using System;
            using FlowWire.Framework.Abstractions;

            namespace TestNamespace
            {
                [FlowState]
                public class MyState 
                { 
                    public int Count { get; set; } 
                }

                [Flow(Mode = FlowMode.Circuit)]
                public partial class MyFlow
                {
                    [Link]
                    public MyState State { get; set; }

                    [Link]
                    public IFlowContext Context { get; set; }

                    [Wire]
                    public FlowCommand Run()
                    {
                        return Command.Finish(State.Count);
                    }
                }
            }";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new FlowGenerator(), source);

        Assert.Empty(diagnostics);
        Assert.Single(generated);
        var code = generated[0];

        Assert.Contains("partial class MyFlow : IFlow", code);
        Assert.Contains("void IFlow.SetState(object state)", code);
        Assert.Contains("this.State = (TestNamespace.MyState)state;", code);
        Assert.Contains("object IFlow.GetState()", code);
        Assert.Contains("return this.State;", code);
        Assert.Contains("void IFlow.SetContext(IFlowContext context)", code);
        Assert.Contains("this.Context = context;", code);
        Assert.Contains("FlowCommand IFlow.Execute()", code);
        Assert.Contains("return this.Run();", code);
    }

    [Fact]
    public void Generate_WithImpulses_GeneratesDispatchLogic()
    {
        var source = @"
            using FlowWire.Framework.Abstractions;

            namespace TestNamespace
            {
                [Flow]
                public partial class SignalFlow
                {
                    [Impulse(""MySignal"")]
                    public void HandleSignal(string data, int count) { }
                }
            }";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new FlowGenerator(), source);

        Assert.Empty(diagnostics);
        var code = generated[0];

        Assert.Contains("void IFlow.DispatchSignal(string signalName, object? arg)", code);
        Assert.Contains("case \"MySignal\":", code);
        Assert.Contains("var args = (object[])arg!;", code);
        Assert.Contains("this.HandleSignal((string)args[0], (int)args[1]);", code);
    }

    [Fact]
    public void Generate_WithProbes_GeneratesDispatchLogic()
    {
        var source = @"
            using FlowWire.Framework.Abstractions;

            namespace TestNamespace
            {
                [Flow]
                public partial class QueryFlow
                {
                    [Probe(""MyQuery"")]
                    public string GetStatus(int detailLevel) { return ""Status""; }
                }
            }";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new FlowGenerator(), source);

        Assert.Empty(diagnostics);
        var code = generated[0];

        Assert.Contains("object? IFlow.DispatchQuery(string queryName, object? arg)", code);
        Assert.Contains("case \"MyQuery\":", code);
        Assert.Contains("return this.GetStatus((int)arg!);", code);
    }

    [Fact]
    public void Generate_WithDependencyInjection_InitializesFields()
    {
        var source = @"
            using FlowWire.Framework.Abstractions;

            namespace TestNamespace
            {
                public class MyService {}

                [Flow(Mode = FlowMode.Circuit)]
                public partial class DiFlow
                {
                    [Link]
                    public MyService Service;

                    [Link]
                    public MyService ServiceProp { get; set; }
                }
            }";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new FlowGenerator(), source);

        Assert.Empty(diagnostics);
        var code = generated[0];

        Assert.Contains("if (this.Service == null) this.Service = context.GetService<TestNamespace.MyService>();", code);
        Assert.Contains("if (this.ServiceProp == null) this.ServiceProp = context.GetService<TestNamespace.MyService>();", code);
    }

    [Fact]
    public void Generate_MemoryMode_IgnoresWireAndDrivers()
    {
        var source = @"
            using FlowWire.Framework.Abstractions;

            namespace TestNamespace
            {
                public class MyService {}

                [Flow(Mode = FlowMode.Memory)]
                public partial class MemoryFlow
                {
                    [Link]
                    public MyService Service;

                    [Wire]
                    public FlowCommand Run()
                    {
                        return Command.Finish();
                    }
                }
            }";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new FlowGenerator(), source);

        Assert.Empty(diagnostics);
        var code = generated[0];

        // Should ignore Wire
        Assert.Contains("return Command.Finish(); // No [Wire] method found", code);
        Assert.DoesNotContain("return this.Run();", code);

        // Should ignore Driver injection
        Assert.DoesNotContain("context.GetService<TestNamespace.MyService>()", code);
    }

    [Fact]
    public void Generate_MinimalFlow_GeneratesValidDefaultImplementations()
    {
        var source = @"
            using FlowWire.Framework.Abstractions;

            namespace TestNamespace
            {
                [Flow]
                public partial class MinimalFlow
                {
                    // No State, No Context, No Wire
                }
            }";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new FlowGenerator(), source);

        Assert.Empty(diagnostics);
        var code = generated[0];

        // Should return new object for GetState
        Assert.Contains("return new object(); // No State property defined", code);
        // Execute should return Finish()
        Assert.Contains("return Command.Finish(); // No [Wire] method found", code);
        // Reset should be empty-ish
        Assert.Contains("void IFlow.Reset()", code);
    }

    [Fact]
    public void Generate_WithMultipleImpulsesAndProbes_GeneratesCorrectSwitchCases()
    {
        var source = @"
            using FlowWire.Framework.Abstractions;

            namespace TestNamespace
            {
                [Flow]
                public partial class ComplexFlow
                {
                    [Impulse(""Sig1"")]
                    public void OnSig1(int a) {}

                    [Impulse(""Sig2"")]
                    public void OnSig2(string b) {}

                    [Probe(""Q1"")]
                    public int GetQ1() { return 1; }

                    [Probe(""Q2"")]
                    public string GetQ2(bool verbose) { return """"; }
                }
            }";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new FlowGenerator(), source);

        Assert.Empty(diagnostics);
        var code = generated[0];

        // Impulses
        Assert.Contains("case \"Sig1\":", code);
        Assert.Contains("this.OnSig1((int)arg!);", code);
        Assert.Contains("case \"Sig2\":", code);
        Assert.Contains("this.OnSig2((string)arg!);", code);

        // Probes
        Assert.Contains("case \"Q1\":", code);
        Assert.Contains("return this.GetQ1();", code);
        Assert.Contains("case \"Q2\":", code);
        Assert.Contains("return this.GetQ2((bool)arg!);", code);
    }

    [Fact]
    public void Generate_WithComplexParameterTypes_GeneratesCorrectCasts()
    {
        var source = @"
            using System.Collections.Generic;
            using FlowWire.Framework.Abstractions;

            namespace TestNamespace
            {
                [Flow]
                public partial class TypedFlow
                {
                    [Impulse(""ComplexSig"")]
                    public void OnImpulse(List<string> items, Dictionary<string, int> map) {}
                }
            }";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new FlowGenerator(), source);

        Assert.Empty(diagnostics);
        var code = generated[0];

        Assert.Contains("var args = (object[])arg!;", code);
        Assert.Contains("this.OnImpulse((System.Collections.Generic.List<string>)args[0], (System.Collections.Generic.Dictionary<string, int>)args[1]);", code);
    }

    [Fact]
    public void Generate_GlobalNamespace_GeneratesRunningCodeWithoutNamespaceBlock()
    {
        var source = @"
            using FlowWire.Framework.Abstractions;

            [Flow]
            public partial class GlobalFlow
            {
            }
            ";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new FlowGenerator(), source);

        Assert.Empty(diagnostics);
        var code = generated[0];

        Assert.DoesNotContain("namespace <global namespace>;", code);
    }

    [Fact]
    public void Generate_MultipleStates_GeneratesCompositeState()
    {
        var source = @"
            using System;
            using FlowWire.Framework.Abstractions;

            namespace TestNamespace
            {
                [FlowState]
                public class CountState { public int Val { get; set; } }

                [FlowState]
                public class StatusState { public string Val { get; set; } }

                [Flow]
                public partial class MultiStateFlow
                {
                    [Link]
                    public CountState Count { get; set; }

                    [Link]
                    public StatusState Status { get; set; }
                }
            }";
        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(new FlowGenerator(), source);

        Assert.Empty(diagnostics);
        Assert.Single(generated);
        var code = generated[0];

        Assert.Contains("class MultiStateFlow_State", code);
        Assert.Contains("public TestNamespace.CountState Count { get; set; }", code);
        Assert.Contains("public TestNamespace.StatusState Status { get; set; }", code);

        // SetState
        Assert.Contains("var s = (MultiStateFlow_State)state;", code);
        Assert.Contains("this.Count = s.Count;", code);
        Assert.Contains("this.Status = s.Status;", code);

        // GetState
        Assert.Contains("return new MultiStateFlow_State", code);
        Assert.Contains("Count = this.Count,", code);
        Assert.Contains("Status = this.Status,", code);

        // Reset
        Assert.Contains("this.Count = new TestNamespace.CountState();", code);
        Assert.Contains("this.Status = new TestNamespace.StatusState();", code);
    }
}
