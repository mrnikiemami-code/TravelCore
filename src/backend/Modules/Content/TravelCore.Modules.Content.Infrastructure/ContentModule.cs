using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modularity;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Content.Infrastructure;

/// <summary>
/// Host composition entry for the Content module (scaffolding — TC-P08-T001).
/// </summary>
public sealed class ContentModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<ContentDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use ContentDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: ContentDbContext.SchemaName);
        });
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        // Product endpoints belong to later P08 tasks.
    }
}
