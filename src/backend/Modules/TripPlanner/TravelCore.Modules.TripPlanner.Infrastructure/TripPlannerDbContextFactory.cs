using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.TripPlanner.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class TripPlannerDbContextFactory : IDesignTimeDbContextFactory<TripPlannerDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_trip_planner_design;Username=travelcore_design;Password=not-a-real-secret";

    public TripPlannerDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TripPlannerDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: TripPlannerDbContext.SchemaName)
            .Options;

        return new TripPlannerDbContext(options);
    }
}
