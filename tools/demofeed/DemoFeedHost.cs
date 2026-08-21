using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TravelCore.Modules.Destination.Contracts;
using TravelCore.Modules.Destination.Infrastructure;
using TravelCore.Modules.Destination.Infrastructure.Services;
using TravelCore.Modules.Media.Contracts;
using TravelCore.Modules.Media.Infrastructure;
using TravelCore.Modules.Media.Infrastructure.Services;
using TravelCore.Modules.Media.Infrastructure.Storage;
using TravelCore.Modules.Party.Contracts;
using TravelCore.Modules.Place.Contracts;
using TravelCore.Modules.Place.Infrastructure;
using TravelCore.Modules.Place.Infrastructure.Services;
using TravelCore.Modules.ReferenceData.Contracts;
using TravelCore.Modules.ReferenceData.Infrastructure;
using TravelCore.Modules.ReferenceData.Infrastructure.Services;
using TravelCore.Modules.Tour.Contracts;
using TravelCore.Modules.Tour.Infrastructure;
using TravelCore.Modules.Tour.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Tools.DemoFeed;

/// <summary>
/// Local DI host for DEMOFEED only — not composed into TravelCore.Api.
/// </summary>
internal static class DemoFeedHost
{
    public const string DemoCodePrefix = "demofeed-";

    public static string ResolveMediaRoot() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, ".local", "demofeed-media"));

    public static IConfiguration BuildConfiguration(string[] args)
    {
        var connectionFromArgs = ExtractConnectionArg(args);
        var mediaRoot = ResolveMediaRoot();

        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.demofeed.json", optional: true, reloadOnChange: false)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Local filesystem adapter — not production object storage / not S3 claims.
                [$"{MediaObjectStorageOptions.SectionName}:UseInMemory"] = "false",
                [$"{MediaObjectStorageOptions.SectionName}:LocalRootPath"] = mediaRoot,
            })
            .AddEnvironmentVariables();

        if (!string.IsNullOrWhiteSpace(connectionFromArgs))
        {
            builder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{TravelCoreConnectionStrings.TravelCore}"] = connectionFromArgs,
            });
        }

        return builder.Build();
    }

    public static ServiceProvider BuildServices(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddSingleton<IClock>(SystemClock.Instance);
        services.AddSingleton<IHostEnvironment>(new DemoFeedHostEnvironment());

        var connectionString = RequireConnectionString(configuration);

        services.AddDbContext<ReferenceDataDbContext>((_, options) =>
        {
            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: ReferenceDataDbContext.SchemaName);
        });

        services.AddDbContext<DestinationDbContext>((_, options) =>
        {
            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: DestinationDbContext.SchemaName);
        });

        services.AddDbContext<PlaceDbContext>((_, options) =>
        {
            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: PlaceDbContext.SchemaName);
        });

        services.AddDbContext<MediaDbContext>((_, options) =>
        {
            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: MediaDbContext.SchemaName);
        });

        services.AddDbContext<TourDbContext>((_, options) =>
        {
            options.UseTravelCorePostgreSql(
                connectionString,
                migrationsHistorySchema: TourDbContext.SchemaName);
        });

        services.AddOptions<MediaObjectStorageOptions>()
            .Bind(configuration.GetSection(MediaObjectStorageOptions.SectionName));
        services.AddOptions<MediaUploadOptions>()
            .Bind(configuration.GetSection(MediaUploadOptions.SectionName));

        services.AddSingleton<IMediaObjectStorage, LocalFileSystemMediaObjectStorage>();

        services.AddScoped<IReferenceDataCatalogQuery, ReferenceDataCatalogQuery>();
        services.AddScoped<IDestinationExistenceQuery, DestinationExistenceQuery>();
        services.AddScoped<DestinationApplicationService>();
        services.AddScoped<IDestinationMediaService>(sp => sp.GetRequiredService<DestinationApplicationService>());
        services.AddScoped<IPlaceService, PlaceApplicationService>();
        services.AddScoped<IMediaAssetReadinessQuery, MediaAssetReadinessQuery>();
        services.AddScoped<IMediaAssetTranslationService, MediaAssetTranslationApplicationService>();
        services.AddScoped<IMediaPresentationService, MediaPresentationApplicationService>();
        services.AddScoped<IMediaUploadService, MediaUploadApplicationService>();

        // Tour owner paths only — Party stub satisfies DI for semantic-link ctor; Agency never set by DEMOFEED.
        services.AddScoped<IPartyReadQuery, DemoFeedPartyReadQueryStub>();
        services.AddScoped<ITourProductService, TourProductService>();
        services.AddScoped<ITourProductSemanticLinkService, TourProductSemanticLinkService>();
        services.AddScoped<ITourProductMediaService, TourProductMediaService>();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true,
        });
    }

    public static string RequireConnectionString(IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString(TravelCoreConnectionStrings.TravelCore);
        if (string.IsNullOrWhiteSpace(cs))
        {
            throw new InvalidOperationException(
                "Connection string 'TravelCore' is required. Set ConnectionStrings__TravelCore " +
                "or pass --connection \"Host=...;Database=TravelCore;Username=...;Password=...\".");
        }

        return cs;
    }

    private static string? ExtractConnectionArg(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] is "--connection" or "-c")
            {
                return args[i + 1];
            }
        }

        return null;
    }

    private sealed class DemoFeedHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "TravelCore.Tools.DemoFeed";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } =
            new PhysicalFileProvider(AppContext.BaseDirectory);
    }

    /// <summary>
    /// DEMOFEED never seeds Agency links; stub keeps Tour semantic-link DI closed without Party schema.
    /// </summary>
    private sealed class DemoFeedPartyReadQueryStub : IPartyReadQuery
    {
        public Task<PartyReadInfo?> GetAsync(Guid partyId, CancellationToken cancellationToken = default)
            => Task.FromResult<PartyReadInfo?>(null);
    }
}
