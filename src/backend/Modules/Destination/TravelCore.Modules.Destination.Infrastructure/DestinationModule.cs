using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modularity;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Destination.Infrastructure;

/// <summary>
/// Host composition entry for the Destination module (scaffolding only).
/// </summary>
public sealed class DestinationModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // CS is resolved when the DbContext is created — host can compose without requiring DB at startup.
        services.AddDbContext<DestinationDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use DestinationDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: DestinationDbContext.SchemaName);
        });
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        // No endpoints in TC-P04-T001 scaffolding.
    }
}
