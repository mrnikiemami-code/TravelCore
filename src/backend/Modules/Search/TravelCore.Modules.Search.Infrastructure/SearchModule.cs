using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modularity;
using TravelCore.Modules.Search.Contracts;
using TravelCore.Modules.Search.Infrastructure.Endpoints;
using TravelCore.Modules.Search.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Search.Infrastructure;

/// <summary>
/// Host composition entry for Search (TC-P15-T001..T007). Schema + public query stub — no physical engine.
/// </summary>
public sealed class SearchModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<SearchDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use SearchDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: SearchDbContext.SchemaName);
        });

        services.AddSingleton<ISearchQueryService, EmptySearchQueryService>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapSearchPublicEndpoints();
    }
}
