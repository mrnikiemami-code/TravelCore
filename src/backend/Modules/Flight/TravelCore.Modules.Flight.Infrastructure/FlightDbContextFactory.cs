using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Flight.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class FlightDbContextFactory : IDesignTimeDbContextFactory<FlightDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_flight_design;Username=travelcore_design;Password=not-a-real-secret";

    public FlightDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FlightDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: FlightDbContext.SchemaName)
            .Options;

        return new FlightDbContext(options);
    }
}
