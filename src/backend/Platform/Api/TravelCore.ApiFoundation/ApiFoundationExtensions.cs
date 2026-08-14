using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace TravelCore.ApiFoundation;

/// <summary>
/// Host-facing API foundation: Problem Details error boundary and System.Text.Json baseline.
/// </summary>
public static class ApiFoundationExtensions
{
    public static IServiceCollection AddTravelCoreApiFoundation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Framework-standard Problem Details; no business error taxonomy yet.
        services.AddProblemDetails();

        // Keep ASP.NET Core System.Text.Json web defaults unless a later task owns a change.
        // Do not configure JsonSerializerOptions merely to create configuration noise.

        return services;
    }

    public static WebApplication UseTravelCoreApiFoundation(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Middleware order: exception handler before status-code pages.
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        return app;
    }
}
