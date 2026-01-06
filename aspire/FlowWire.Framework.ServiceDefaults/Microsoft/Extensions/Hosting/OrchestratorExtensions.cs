using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.Hosting;

public static class OrchestratorExtensions
{
    public static TBuilder AddOrchestratorDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.AddServiceDefaults();
        builder.AddOrchestratorHealthChecks();

        return builder;
    }

    public static TBuilder AddOrchestratorHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
        // Future: Add persistence checks (SQL/Redis) here
        return builder;
    }
}