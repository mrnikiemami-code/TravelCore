using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;
using TravelCore.Modularity;
using TravelCore.Modules.TripPlanner.Contracts;
using TravelCore.Modules.TripPlanner.Infrastructure.Endpoints;
using TravelCore.Modules.TripPlanner.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.TripPlanner.Infrastructure;

/// <summary>
/// Host composition entry for TripPlanner (TC-P18-T001 / P18-R1 scaffolding only).
/// </summary>
public sealed class TripPlannerModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<TripPlannerDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use TripPlannerDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: TripPlannerDbContext.SchemaName);
        });

        services.TryAddSingleton<IClock>(SystemClock.Instance);
        services.AddScoped<ITripPlannerPublicCommand, TripPlannerPublicCommand>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapTripPlannerPublicEndpoints();
    }
}
