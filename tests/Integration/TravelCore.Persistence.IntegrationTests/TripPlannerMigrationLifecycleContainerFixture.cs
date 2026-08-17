using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.TripPlanner.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for TripPlanner module migration lifecycle (TC-P18-T001).
/// </summary>
public sealed class TripPlannerMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_trip_planner_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public TripPlannerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TripPlannerDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: TripPlannerDbContext.SchemaName)
            .Options;
        return new TripPlannerDbContext(options);
    }
}

[CollectionDefinition(nameof(TripPlannerMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class TripPlannerMigrationLifecycleCollection : ICollectionFixture<TripPlannerMigrationLifecycleContainerFixture>;
