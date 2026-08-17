using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Ugc.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class UgcDbContextFactory : IDesignTimeDbContextFactory<UgcDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_ugc_design;Username=travelcore_design;Password=not-a-real-secret";

    public UgcDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<UgcDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: UgcDbContext.SchemaName)
            .Options;

        return new UgcDbContext(options);
    }
}
