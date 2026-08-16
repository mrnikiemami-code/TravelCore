using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Media.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for Media module migration lifecycle (TC-P06-T002).
/// </summary>
public sealed class MediaMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_media_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public MediaDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MediaDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: MediaDbContext.SchemaName)
            .Options;
        return new MediaDbContext(options);
    }
}

[CollectionDefinition(nameof(MediaMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class MediaMigrationLifecycleCollection : ICollectionFixture<MediaMigrationLifecycleContainerFixture>;
