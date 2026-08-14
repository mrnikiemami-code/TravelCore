using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace TravelCore.Health;

/// <summary>
/// Host-facing health foundation: framework health checks with explicit liveness vs readiness endpoints.
/// </summary>
public static class HealthExtensions
{
    public const string LivenessPath = "/health/live";
    public const string ReadinessPath = "/health/ready";

    /// <summary>
    /// Registers the ASP.NET Core health-check services. Does not invent dependency probes.
    /// </summary>
    public static IServiceCollection AddTravelCoreHealth(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHealthChecks();

        return services;
    }

    /// <summary>
    /// Maps operational liveness and readiness endpoints. Excluded from OpenAPI (not business API).
    /// </summary>
    public static IEndpointRouteBuilder MapTravelCoreHealth(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        // Liveness: process viability only — execute ZERO registered health checks.
        // Untagged future dependency checks must not contaminate this endpoint.
        endpoints.MapHealthChecks(LivenessPath, new HealthCheckOptions
            {
                Predicate = static _ => false
            })
            .WithMetadata(new ExcludeFromDescriptionAttribute());

        // Readiness: only checks tagged "ready". Empty set is Healthy until dependencies register.
        endpoints.MapHealthChecks(ReadinessPath, new HealthCheckOptions
            {
                Predicate = static registration =>
                    registration.Tags.Contains(TravelCoreHealthTags.Ready)
            })
            .WithMetadata(new ExcludeFromDescriptionAttribute());

        return endpoints;
    }
}
