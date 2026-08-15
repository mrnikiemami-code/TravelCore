using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modularity;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.ReferenceData.Infrastructure;

/// <summary>
/// Host composition entry for the ReferenceData module (scaffolding only).
/// </summary>
public sealed class ReferenceDataModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // CS is resolved when the DbContext is created — host can compose without requiring DB at startup.
        services.AddDbContext<ReferenceDataDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use ReferenceDataDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: ReferenceDataDbContext.SchemaName);
        });
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        // No endpoints in TC-P04-T001 scaffolding.
    }
}
