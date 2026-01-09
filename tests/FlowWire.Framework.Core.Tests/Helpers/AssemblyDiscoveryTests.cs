using System.Reflection;
using FlowWire.Framework.Abstractions;
using FlowWire.Framework.Core.Helpers;

[assembly: FlowWireAssembly]

namespace FlowWire.Framework.Core.Tests.Helpers;

public class AssemblyDiscoveryTests
{
    [Fact]
    public void FindFlowWireAssemblies_ShouldFindCurrentAssembly()
    {
        // Act
        var assemblies = AssemblyDiscovery.FindFlowWireAssemblies();

        // Assert
        // We expect the current assembly (FlowWire.Framework.Core.Tests) to be found
        // because it has [assembly: FlowWireAssembly] and references Abstractions.
        Assert.Contains(assemblies, a => a == typeof(AssemblyDiscoveryTests).Assembly);
    }

    [Fact]
    public void FindFlowWireAssemblies_ShouldNotContainSystemAssemblies()
    {
        // Act
        var assemblies = AssemblyDiscovery.FindFlowWireAssemblies();

        // Assert
        // Verify we aren't pulling in random system assemblies
        Assert.DoesNotContain(assemblies, a => a.GetName().Name?.StartsWith("System.") == true);
    }

    [Fact]
    public void FindFlowWireAssemblies_ShouldNotContainAbstractionsAssembly()
    {
        // Act
        var assemblies = AssemblyDiscovery.FindFlowWireAssemblies();

        // Assert
        // FlowWire.Framework.Abstractions is a candidate but does not have the attribute, so it should be filtered out.
        Assert.DoesNotContain(assemblies, a => a == typeof(FlowWireAssemblyAttribute).Assembly);
    }

    [Fact]
    public void FindFlowWireAssemblies_ShouldOnlyContainAssembliesWithAttribute()
    {
        // Act
        var assemblies = AssemblyDiscovery.FindFlowWireAssemblies();

        // Assert
        Assert.All(assemblies, a => Assert.NotNull(a.GetCustomAttribute<FlowWireAssemblyAttribute>()));
    }

    [Fact]
    public void FindFlowWireAssemblies_ReturnsUniqueAssemblies()
    {
        // Act
        var assemblies = AssemblyDiscovery.FindFlowWireAssemblies();

        // Assert
        Assert.Equal(assemblies.Length, assemblies.Distinct().Count());
    }

    [Fact]
    public void FindFlowWireAssemblies_ShouldNotContainMscorlib()
    {
        // Act
        var assemblies = AssemblyDiscovery.FindFlowWireAssemblies();

        // Assert
        var objectAssembly = typeof(object).Assembly;
        Assert.DoesNotContain(assemblies, a => a == objectAssembly);
    }
}
