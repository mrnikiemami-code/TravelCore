using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;
using TravelCore.Modularity;
using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Infrastructure.Endpoints;
using TravelCore.Modules.Seo.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Seo.Infrastructure;

/// <summary>
/// Host composition entry for the SEO module.
/// </summary>
public sealed class SeoModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.TryAddSingleton<IClock>(SystemClock.Instance);

        services.AddDbContext<SeoDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use SeoDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: SeoDbContext.SchemaName);
        });

        services.AddScoped<SeoRedirectApplicationService>();
        services.AddScoped<ISeoRedirectService>(sp => sp.GetRequiredService<SeoRedirectApplicationService>());
        services.AddScoped<ISeoRouteService, SeoRouteApplicationService>();
        services.AddScoped<ISeoIndexPolicyService, SeoIndexPolicyApplicationService>();
        services.AddScoped<ISeoHreflangService, SeoHreflangApplicationService>();
        services.AddScoped<ISeoMetadataService, SeoMetadataApplicationService>();
        services.AddScoped<ISeoStructuredDataService, SeoStructuredDataApplicationService>();
        services.AddScoped<ISeoSitemapService, SeoSitemapApplicationService>();
        services.AddScoped<ISeoDestinationPublicationService, SeoDestinationPublicationService>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapSeoEndpoints();
    }
}
