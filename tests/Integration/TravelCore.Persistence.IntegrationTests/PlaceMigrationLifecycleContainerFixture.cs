using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Place.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for Place module migration lifecycle (TC-P07-T002).
/// </summary>
public sealed class PlaceMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_place_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public PlaceDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PlaceDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: PlaceDbContext.SchemaName)
            .Options;
        return new PlaceDbContext(options);
    }
}

[CollectionDefinition(nameof(PlaceMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class PlaceMigrationLifecycleCollection : ICollectionFixture<PlaceMigrationLifecycleContainerFixture>;
