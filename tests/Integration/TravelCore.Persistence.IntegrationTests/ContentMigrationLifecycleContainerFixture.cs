using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Content.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for Content module migration lifecycle (TC-P08-T002).
/// </summary>
public sealed class ContentMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_content_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public ContentDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: ContentDbContext.SchemaName)
            .Options;
        return new ContentDbContext(options);
    }
}

[CollectionDefinition(nameof(ContentMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class ContentMigrationLifecycleCollection : ICollectionFixture<ContentMigrationLifecycleContainerFixture>;
