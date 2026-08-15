using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using TravelCore.Persistence.PostgreSql;
using TravelCore.PersistenceFixture;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for T017 migration lifecycle (no auto-migrate on start).
/// Separate from T016 suite which migrates during fixture InitializeAsync.
/// </summary>
public sealed class MigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_p01_migration_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
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

[CollectionDefinition(nameof(MigrationLifecycleCollection), DisableParallelization = true)]
public sealed class MigrationLifecycleCollection : ICollectionFixture<MigrationLifecycleContainerFixture>;
