using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.Modules.Tour.Infrastructure;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Fake non-secret connection string.
/// </summary>
public sealed class TourDbContextFactory : IDesignTimeDbContextFactory<TourDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_tour_design;Username=travelcore_design;Password=not-a-real-secret";

    public TourDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TourDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: TourDbContext.SchemaName)
            .Options;

        return new TourDbContext(options);
    }
}
