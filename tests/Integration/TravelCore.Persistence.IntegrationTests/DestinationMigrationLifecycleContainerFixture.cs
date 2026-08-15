using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Destination.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for Destination module migration lifecycle (TC-P04-T003).
/// </summary>
public sealed class DestinationMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_destination_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public DestinationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<DestinationDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: DestinationDbContext.SchemaName)
            .Options;
        return new DestinationDbContext(options);
    }
}

[CollectionDefinition(nameof(DestinationMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class DestinationMigrationLifecycleCollection : ICollectionFixture<DestinationMigrationLifecycleContainerFixture>;
