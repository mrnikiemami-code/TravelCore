using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Notification.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for Notification module migration lifecycle (TC-P25-T004).
/// </summary>
public sealed class NotificationMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_notification_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public NotificationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: NotificationDbContext.SchemaName)
            .Options;
        return new NotificationDbContext(options);
    }
}

[CollectionDefinition(nameof(NotificationMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class NotificationMigrationLifecycleCollection
    : ICollectionFixture<NotificationMigrationLifecycleContainerFixture>;
