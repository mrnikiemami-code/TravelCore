using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace TravelCore.Observability;

/// <summary>
/// Host-facing observability foundation: correlation middleware and logging-scope conventions.
/// </summary>
public static class ObservabilityExtensions
{
    /// <summary>
    /// Marker registration for the observability capability. No third-party providers.
    /// </summary>
    public static IServiceCollection AddTravelCoreObservability(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }

    /// <summary>
    /// Enables request correlation and structured logging scope early in the pipeline.
    /// </summary>
    public static IApplicationBuilder UseTravelCoreObservability(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<CorrelationMiddleware>();
    }
}
