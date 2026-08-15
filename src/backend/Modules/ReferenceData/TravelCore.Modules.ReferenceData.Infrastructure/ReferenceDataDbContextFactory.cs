using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.ReferenceData.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class ReferenceDataDbContextFactory : IDesignTimeDbContextFactory<ReferenceDataDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_referencedata_design;Username=travelcore_design;Password=not-a-real-secret";

    public ReferenceDataDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ReferenceDataDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: ReferenceDataDbContext.SchemaName)
            .Options;

        return new ReferenceDataDbContext(options);
    }
}
