using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Seo.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for SEO module migration lifecycle (TC-P05-T002).
/// </summary>
public sealed class SeoMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_seo_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public SeoDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SeoDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: SeoDbContext.SchemaName)
            .Options;
        return new SeoDbContext(options);
    }
}

[CollectionDefinition(nameof(SeoMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class SeoMigrationLifecycleCollection : ICollectionFixture<SeoMigrationLifecycleContainerFixture>;
