using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using TravelCore.Persistence.PostgreSql;
using TravelCore.PersistenceFixture;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Ephemeral PostgreSQL 18.6 container for P01 integration evidence only.
/// Applies fixture migrations once as test-environment setup (not T017 acceptance).
/// </summary>
public sealed class PostgreSqlContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_p01_integration")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateDbContext();
        await PersistenceFixtureMigrator.MigrateAsync(db);
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public PersistenceFixtureDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PersistenceFixtureDbContext>()
            .UseTravelCorePostgreSql(
                ConnectionString,
                migrationsHistorySchema: PersistenceFixtureDbContext.SchemaName)
            .Options;
        return new PersistenceFixtureDbContext(options);
    }
}
