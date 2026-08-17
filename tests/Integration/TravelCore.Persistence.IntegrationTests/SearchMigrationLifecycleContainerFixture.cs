using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Search.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for Search module migration lifecycle (TC-P15-T001).
/// </summary>
public sealed class SearchMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_search_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public SearchDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SearchDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: SearchDbContext.SchemaName)
            .Options;
        return new SearchDbContext(options);
    }
}

[CollectionDefinition(nameof(SearchMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class SearchMigrationLifecycleCollection : ICollectionFixture<SearchMigrationLifecycleContainerFixture>;
