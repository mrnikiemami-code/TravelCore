using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Pricing.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for Pricing module migration lifecycle (TC-P12-T001).
/// </summary>
public sealed class PricingMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_pricing_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public PricingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PricingDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: PricingDbContext.SchemaName)
            .Options;
        return new PricingDbContext(options);
    }
}

[CollectionDefinition(nameof(PricingMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class PricingMigrationLifecycleCollection : ICollectionFixture<PricingMigrationLifecycleContainerFixture>;
