using System.Reflection;
using FlowWire.Framework.Abstractions;
using Microsoft.Extensions.DependencyModel;

namespace FlowWire.Framework.Core.Helpers;

static internal class AssemblyDiscovery
{
    public static Assembly[] FindFlowWireAssemblies()
    {
        var assemblies = new HashSet<Assembly>();

        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null)
        {
            assemblies.Add(entryAssembly);
        }

        // Supports "Lazy Loading"
        var deps = DependencyContext.Default;
        if (deps != null)
        {
            foreach (var lib in deps.RuntimeLibraries)
            {
                if (IsCandidateLibrary(lib))
                {
                    try
                    {
                        var assembly = Assembly.Load(new AssemblyName(lib.Name));
                        assemblies.Add(assembly);
                    }
                    catch
                    {
                        // Ignore load failures
                    }
                }
            }
        }
        else
        {
            // Fallback for environments without DependencyContext (e.g. some Unit Tests)
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                assemblies.Add(a);
            }
        }

        return [.. assemblies.Where(a => a.GetCustomAttribute<FlowWireAssemblyAttribute>() != null)];
    }

    private static bool IsCandidateLibrary(RuntimeLibrary lib)
    {
        // This prevents us from loading System.* or Microsoft.* unless necessary
        return lib.Dependencies.Any(d => d.Name == "FlowWire.Framework.Abstractions")
               || lib.Name == "FlowWire.Framework.Abstractions";
    }
}