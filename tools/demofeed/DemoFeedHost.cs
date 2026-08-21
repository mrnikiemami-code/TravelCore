using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using TravelCore.Modules.Destination.Infrastructure;
using TravelCore.Modules.Destination.Infrastructure.Services;
using TravelCore.Modules.ReferenceData.Contracts;
using TravelCore.Modules.ReferenceData.Infrastructure;
using TravelCore.Modules.ReferenceData.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Tools.DemoFeed;

/// <summary>
/// Local DI host for DEMOFEED only — not composed into TravelCore.Api.
/// </summary>
internal static class DemoFeedHost
{
    public const string DemoCodePrefix = "demofeed-";

    public static IConfiguration BuildConfiguration(string[] args)
    {
        var connectionFromArgs = ExtractConnectionArg(args);
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.demofeed.json", optional: true, reloadOnChange: false)
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

        services.AddDbContext<ReferenceDataDbContext>((_, options) =>
        {
            options.UseTravelCorePostgreSql(
                RequireConnectionString(configuration),
                migrationsHistorySchema: ReferenceDataDbContext.SchemaName);
        });

        services.AddDbContext<DestinationDbContext>((_, options) =>
        {
            options.UseTravelCorePostgreSql(
                RequireConnectionString(configuration),
                migrationsHistorySchema: DestinationDbContext.SchemaName);
        });

        services.AddScoped<IReferenceDataCatalogQuery, ReferenceDataCatalogQuery>();
        services.AddScoped<DestinationApplicationService>();

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
}
