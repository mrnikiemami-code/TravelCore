using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Destination.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class DestinationDbContextFactory : IDesignTimeDbContextFactory<DestinationDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_destination_design;Username=travelcore_design;Password=not-a-real-secret";

    public DestinationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DestinationDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: DestinationDbContext.SchemaName)
            .Options;

        return new DestinationDbContext(options);
    }
}
