using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TravelCore.Persistence.PostgreSql;

namespace TravelCore.PersistenceFixture;

/// <summary>
/// Design-time factory for EF tooling only (dotnet-ef). Not production runtime infrastructure.
/// Uses a clearly fake connection string so options/model metadata can be built without a live server.
/// </summary>
public sealed class PersistenceFixtureDbContextFactory
    : IDesignTimeDbContextFactory<PersistenceFixtureDbContext>
{
    // فقط برای ساخت مدل در tooling؛ نه پیکربندی runtime و نه اتصال واقعی.
    private const string DesignTimeConnectionString =
        "Host=localhost;Database=travelcore_p01_fixture_design;Username=travelcore_design;Password=not-a-real-secret";

    public PersistenceFixtureDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PersistenceFixtureDbContext>()
            .UseTravelCorePostgreSql(
                DesignTimeConnectionString,
                migrationsHistorySchema: PersistenceFixtureDbContext.SchemaName)
            .Options;

        return new PersistenceFixtureDbContext(options);
    }
}
