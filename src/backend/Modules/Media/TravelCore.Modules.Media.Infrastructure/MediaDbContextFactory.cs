using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Media.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class MediaDbContextFactory : IDesignTimeDbContextFactory<MediaDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_media_design;Username=travelcore_design;Password=not-a-real-secret";

    public MediaDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MediaDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: MediaDbContext.SchemaName)
            .Options;

        return new MediaDbContext(options);
    }
}
