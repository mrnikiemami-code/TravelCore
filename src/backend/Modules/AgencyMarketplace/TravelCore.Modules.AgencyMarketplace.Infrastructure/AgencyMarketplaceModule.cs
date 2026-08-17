using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modularity;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure;

/// <summary>
/// Host composition entry for the Agency Marketplace module (scaffolding — TC-P13-T001).
/// </summary>
public sealed class AgencyMarketplaceModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<AgencyMarketplaceDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use AgencyMarketplaceDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: AgencyMarketplaceDbContext.SchemaName);
        });
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        // Product endpoints belong to later P13 tasks.
    }
}
