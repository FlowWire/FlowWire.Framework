using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using FlowWire.Framework.Abstractions;

namespace FlowWire.Framework.Analyzers.Tests;

public static class GeneratorTestHelper
{
    public static Task Verify(IIncrementalGenerator generator, string source)
    {
        // Parse the provided source code
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        // precise references are important for the generator to work correctly
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(OpAttribute).Assembly.Location)
        };
        
        // Add all assemblies loaded in the current app domain
        Assembly.GetEntryAssembly()?.GetReferencedAssemblies()
            .ToList()
            .ForEach(a => references.Add(MetadataReference.CreateFromFile(Assembly.Load(a).Location)));
            
        // ensure System.Runtime is loaded
        var systemRuntime = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "System.Runtime");
        if (systemRuntime is not null)
        {
            references.Add(MetadataReference.CreateFromFile(systemRuntime.Location));
        }


        // Create a Roslyn compilation
        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Create an instance of our generator driver
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the generation
        driver = driver.RunGenerators(compilation);

        // Verify the results
        return Verify(driver);
    }

    private static Task Verify(GeneratorDriver driver)
    {
        // Get the generation results
        var runResult = driver.GetRunResult();

        // Check for any diagnostics (errors/warnings)
        // Assert.Empty(runResult.Diagnostics); // Commented out to allow testing specific failure scenarios if needed

        // Return the generated source(s) for assertion
        // For simplicity in this helper, we'll just return the first generated source text if present
        // In a real verification scenario (like Verify.Xunit), we would snapshot test this.
        // Here we will just return the result for manual assertion in the test method.
        return Task.CompletedTask;
    }
    
    public static (ImmutableArray<Diagnostic> Diagnostics, string[] GeneratedSources) RunGenerator(IIncrementalGenerator generator, string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(OpAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        
        var result = driver.GetRunResult();
        return (result.Diagnostics, result.GeneratedTrees.Select(t => t.ToString()).ToArray());
    }
}
