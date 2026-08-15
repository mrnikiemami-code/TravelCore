using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using TravelCore.Modules.Identity.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

public sealed class IdentityMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_identity_migration_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public IdentityDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseTravelCorePostgreSql(
                ConnectionString,
                migrationsHistorySchema: IdentityDbContext.SchemaName)
            .Options;
        return new IdentityDbContext(options);
    }
}

[CollectionDefinition(nameof(IdentityMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class IdentityMigrationLifecycleCollection : ICollectionFixture<IdentityMigrationLifecycleContainerFixture>;
