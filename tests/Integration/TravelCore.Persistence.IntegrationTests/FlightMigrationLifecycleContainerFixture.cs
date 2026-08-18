using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Flight.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for Flight module migration lifecycle (TC-P22-T001).
/// </summary>
public sealed class FlightMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_flight_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public FlightDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<FlightDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: FlightDbContext.SchemaName)
            .Options;
        return new FlightDbContext(options);
    }
}

[CollectionDefinition(nameof(FlightMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class FlightMigrationLifecycleCollection : ICollectionFixture<FlightMigrationLifecycleContainerFixture>;
