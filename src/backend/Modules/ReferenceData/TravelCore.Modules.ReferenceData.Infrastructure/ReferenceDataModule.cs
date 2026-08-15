using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modularity;
using TravelCore.Modules.ReferenceData.Contracts;
using TravelCore.Modules.ReferenceData.Infrastructure.Endpoints;
using TravelCore.Modules.ReferenceData.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.ReferenceData.Infrastructure;

/// <summary>
/// Host composition entry for the ReferenceData module.
/// </summary>
public sealed class ReferenceDataModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<ReferenceDataDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use ReferenceDataDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: ReferenceDataDbContext.SchemaName);
        });

        services.AddScoped<IReferenceDataCatalogQuery, ReferenceDataCatalogQuery>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapReferenceDataEndpoints();
    }
}
