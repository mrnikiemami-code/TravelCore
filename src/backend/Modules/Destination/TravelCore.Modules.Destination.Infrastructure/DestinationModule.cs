using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;
using TravelCore.Modularity;
using TravelCore.Modules.Destination.Contracts;
using TravelCore.Modules.Destination.Infrastructure.Endpoints;
using TravelCore.Modules.Destination.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Destination.Infrastructure;

/// <summary>
/// Host composition entry for the Destination module.
/// </summary>
public sealed class DestinationModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.TryAddSingleton<IClock>(SystemClock.Instance);

        services.AddDbContext<DestinationDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use DestinationDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: DestinationDbContext.SchemaName);
        });

        services.AddScoped<DestinationApplicationService>();
        services.AddScoped<IDestinationReadQuery, DestinationReadQuery>();
        services.AddScoped<IDestinationExistenceQuery, DestinationExistenceQuery>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapDestinationEndpoints();
    }
}
