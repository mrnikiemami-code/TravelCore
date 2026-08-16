using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Place.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class PlaceDbContextFactory : IDesignTimeDbContextFactory<PlaceDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_place_design;Username=travelcore_design;Password=not-a-real-secret";

    public PlaceDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PlaceDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: PlaceDbContext.SchemaName)
            .Options;

        return new PlaceDbContext(options);
    }
}
