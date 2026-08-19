using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.DynamicPackage.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for DynamicPackage module migration lifecycle (TC-P23-T001).
/// </summary>
public sealed class DynamicPackageMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_dynamic_package_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public DynamicPackageDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<DynamicPackageDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: DynamicPackageDbContext.SchemaName)
            .Options;
        return new DynamicPackageDbContext(options);
    }
}

[CollectionDefinition(nameof(DynamicPackageMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class DynamicPackageMigrationLifecycleCollection
    : ICollectionFixture<DynamicPackageMigrationLifecycleContainerFixture>;
