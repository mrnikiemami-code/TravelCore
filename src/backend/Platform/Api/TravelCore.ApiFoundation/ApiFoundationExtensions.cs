using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime.Serialization.SystemTextJson;
using TravelCore.Time;

namespace TravelCore.ApiFoundation;

/// <summary>
/// Host-facing API foundation: Problem Details, System.Text.Json baseline, OpenAPI, and NodaTime JSON.
/// </summary>
public static class ApiFoundationExtensions
{
    public static IServiceCollection AddTravelCoreApiFoundation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Framework-standard Problem Details; no business error taxonomy yet.
        services.AddProblemDetails();

        // Official ASP.NET Core OpenAPI document generation (runtime).
        services.AddOpenApi();

        // System.Text.Json with official NodaTime converters (IANA/TZDB).
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.ConfigureForNodaTime(TravelCoreTemporal.TimeZones);
        });

        return services;
    }

    public static WebApplication UseTravelCoreApiFoundation(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Middleware order: exception handler before status-code pages.
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        // Development-only OpenAPI document exposure for P01.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        return app;
    }
}
