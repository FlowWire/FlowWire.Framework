using BenchmarkDotNet.Running;
using FlowWire.Framework.Abstractions;

[assembly: FlowWireAssembly]

namespace FlowWire.Framework.Benchmarks;

internal class Program
{
    private static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
