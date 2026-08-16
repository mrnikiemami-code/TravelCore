using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Tour.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for Tour module migration lifecycle (TC-P09-T002).
/// </summary>
public sealed class TourMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_tour_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public TourDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TourDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: TourDbContext.SchemaName)
            .Options;
        return new TourDbContext(options);
    }
}

[CollectionDefinition(nameof(TourMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class TourMigrationLifecycleCollection : ICollectionFixture<TourMigrationLifecycleContainerFixture>;
