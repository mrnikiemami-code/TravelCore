using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;
using TravelCore.Modularity;
using TravelCore.Modules.Tour.Contracts;
using TravelCore.Modules.Tour.Infrastructure.Endpoints;
using TravelCore.Modules.Tour.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Tour.Infrastructure;

/// <summary>
/// Host composition entry for the Tour module.
/// </summary>
public sealed class TourModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<TourDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use TourDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: TourDbContext.SchemaName);
        });

        services.TryAddSingleton<IClock>(SystemClock.Instance);
        services.AddScoped<ITourProductSemanticLinkService, TourProductSemanticLinkService>();
        services.AddScoped<ITourProductCatalogFactService, TourProductCatalogFactService>();
        services.AddScoped<ITourProductMediaService, TourProductMediaService>();
        services.AddScoped<ITourProductService, TourProductService>();
        services.AddScoped<IExperienceItineraryStopLinkService, ExperienceItineraryStopLinkService>();
        services.AddScoped<IExperienceGuideAssignmentService, ExperienceGuideAssignmentService>();
        services.AddScoped<IExperienceMediaService, ExperienceMediaService>();
        services.AddScoped<IExperienceCatalogService, ExperienceCatalogService>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapTourEndpoints();
    }
}
