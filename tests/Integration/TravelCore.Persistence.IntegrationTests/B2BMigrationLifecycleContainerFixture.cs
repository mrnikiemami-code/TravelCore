using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.B2B.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for B2B module migration lifecycle (TC-P24-T001).
/// </summary>
public sealed class B2BMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_b2b_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public B2BDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<B2BDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: B2BDbContext.SchemaName)
            .Options;
        return new B2BDbContext(options);
    }
}

[CollectionDefinition(nameof(B2BMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class B2BMigrationLifecycleCollection
    : ICollectionFixture<B2BMigrationLifecycleContainerFixture>;
