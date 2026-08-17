using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;
using TravelCore.Modularity;
using TravelCore.Modules.Content.Contracts;
using TravelCore.Modules.Content.Infrastructure.Endpoints;
using TravelCore.Modules.Content.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Content.Infrastructure;

/// <summary>
/// Host composition entry for the Content module.
/// </summary>
public sealed class ContentModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.TryAddSingleton<IClock>(SystemClock.Instance);

        services.AddDbContext<ContentDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use ContentDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: ContentDbContext.SchemaName);
        });

        services.AddScoped<IContentItemService, ContentItemApplicationService>();
        services.AddScoped<IContentTaxonomyService, ContentTaxonomyApplicationService>();
        services.AddScoped<IContentBlockService, ContentBlockApplicationService>();
        services.AddScoped<IRelatedContentPublicQuery, RelatedContentPublicQuery>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapContentEndpoints();
    }
}
