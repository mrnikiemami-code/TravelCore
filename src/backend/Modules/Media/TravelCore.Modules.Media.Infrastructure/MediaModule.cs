using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;
using TravelCore.Modularity;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Infrastructure.Endpoints;
using TravelCore.Modules.Media.Infrastructure.Processing;
using TravelCore.Modules.Media.Infrastructure.Services;
using TravelCore.Modules.Media.Infrastructure.Storage;
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

        services.AddOptions<MediaObjectStorageOptions>()
            .Bind(configuration.GetSection(MediaObjectStorageOptions.SectionName));
        services.AddOptions<MediaUploadOptions>()
            .Bind(configuration.GetSection(MediaUploadOptions.SectionName));

        var useInMemory = configuration.GetValue(
            $"{MediaObjectStorageOptions.SectionName}:UseInMemory",
            defaultValue: false);
        if (useInMemory)
        {
            services.AddSingleton<IMediaObjectStorage, InMemoryMediaObjectStorage>();
        }
        else
        {
            services.AddSingleton<IMediaObjectStorage, LocalFileSystemMediaObjectStorage>();
        }

        services.AddScoped<IMediaAssetService, MediaAssetApplicationService>();
        services.AddScoped<IMediaObjectBindingService, MediaObjectBindingService>();
        services.AddScoped<IMediaUploadService, MediaUploadApplicationService>();
        services.AddScoped<IMediaFocalPointService, MediaFocalPointApplicationService>();
        services.AddScoped<IMediaAssetTranslationService, MediaAssetTranslationApplicationService>();
        services.AddScoped<IMediaContentDeliveryService, MediaContentDeliveryService>();
        services.AddScoped<IMediaPresentationService, MediaPresentationApplicationService>();
        services.AddSingleton<ImageSharpMediaVariantProcessor>();
        services.AddScoped<IMediaVariantProcessingService, MediaVariantApplicationService>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapMediaEndpoints();
    }
}
