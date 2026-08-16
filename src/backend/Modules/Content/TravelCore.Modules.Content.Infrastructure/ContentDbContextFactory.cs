using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Content.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class ContentDbContextFactory : IDesignTimeDbContextFactory<ContentDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_content_design;Username=travelcore_design;Password=not-a-real-secret";

    public ContentDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: ContentDbContext.SchemaName)
            .Options;

        return new ContentDbContext(options);
    }
}
