using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using TravelCore.Modules.Party.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for Party module migration lifecycle (TC-P03-T002).
/// </summary>
public sealed class PartyMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_party_migration_lifecycle")
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

    public PartyDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PartyDbContext>()
            .UseTravelCorePostgreSql(
                ConnectionString,
                migrationsHistorySchema: PartyDbContext.SchemaName)
            .Options;
        return new PartyDbContext(options);
    }
}

[CollectionDefinition(nameof(PartyMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class PartyMigrationLifecycleCollection : ICollectionFixture<PartyMigrationLifecycleContainerFixture>;
