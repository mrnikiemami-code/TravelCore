using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Analytics.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for Analytics module migration lifecycle (TC-P27-T004).
/// </summary>
public sealed class AnalyticsMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_analytics_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public AnalyticsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: AnalyticsDbContext.SchemaName)
            .Options;
        return new AnalyticsDbContext(options);
    }
}

[CollectionDefinition(nameof(AnalyticsMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class AnalyticsMigrationLifecycleCollection
    : ICollectionFixture<AnalyticsMigrationLifecycleContainerFixture>;
