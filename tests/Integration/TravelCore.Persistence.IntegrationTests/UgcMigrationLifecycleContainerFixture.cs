using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Ugc.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for UGC module migration lifecycle (TC-P16-T001).
/// </summary>
public sealed class UgcMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_ugc_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public UgcDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<UgcDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: UgcDbContext.SchemaName)
            .Options;
        return new UgcDbContext(options);
    }
}

[CollectionDefinition(nameof(UgcMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class UgcMigrationLifecycleCollection : ICollectionFixture<UgcMigrationLifecycleContainerFixture>;
