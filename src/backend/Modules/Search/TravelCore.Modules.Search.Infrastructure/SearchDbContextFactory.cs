using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Search.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class SearchDbContextFactory : IDesignTimeDbContextFactory<SearchDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_search_design;Username=travelcore_design;Password=not-a-real-secret";

    public SearchDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SearchDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: SearchDbContext.SchemaName)
            .Options;

        return new SearchDbContext(options);
    }
}
