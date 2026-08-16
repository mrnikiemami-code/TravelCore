using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Seo.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class SeoDbContextFactory : IDesignTimeDbContextFactory<SeoDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_seo_design;Username=travelcore_design;Password=not-a-real-secret";

    public SeoDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SeoDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: SeoDbContext.SchemaName)
            .Options;

        return new SeoDbContext(options);
    }
}
