using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using TravelCore.Modules.ReferenceData.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for ReferenceData module migration lifecycle (TC-P04-T002).
/// </summary>
public sealed class ReferenceDataMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_referencedata_migration_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public ReferenceDataDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ReferenceDataDbContext>()
            .UseTravelCorePostgreSql(
                ConnectionString,
                migrationsHistorySchema: ReferenceDataDbContext.SchemaName)
            .Options;
        return new ReferenceDataDbContext(options);
    }
}

[CollectionDefinition(nameof(ReferenceDataMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class ReferenceDataMigrationLifecycleCollection : ICollectionFixture<ReferenceDataMigrationLifecycleContainerFixture>;
