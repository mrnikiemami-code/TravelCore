using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;
using TravelCore.Modularity;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Media.Infrastructure;

/// <summary>
/// Host composition entry for the Media module.
/// </summary>
public sealed class MediaModule : ITravelCoreModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.TryAddSingleton<IClock>(SystemClock.Instance);

        services.AddDbContext<MediaDbContext>((_, options) =>
        {
            var connectionString = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore)
                ?? throw new InvalidOperationException(
                    $"Connection string '{TravelCoreConnectionStrings.TravelCore}' is required to use MediaDbContext.");

            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: MediaDbContext.SchemaName);
        });

        services.AddScoped<IMediaAssetService, MediaAssetApplicationService>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        // Product Media HTTP endpoints deferred (upload/Admin = later P06 tasks).
    }
}
