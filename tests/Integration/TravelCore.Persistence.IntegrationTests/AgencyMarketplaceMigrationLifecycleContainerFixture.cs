using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.AgencyMarketplace.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for Agency Marketplace module migration lifecycle (TC-P13-T001).
/// </summary>
public sealed class AgencyMarketplaceMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_agency_marketplace_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public AgencyMarketplaceDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AgencyMarketplaceDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: AgencyMarketplaceDbContext.SchemaName)
            .Options;
        return new AgencyMarketplaceDbContext(options);
    }
}

[CollectionDefinition(nameof(AgencyMarketplaceMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class AgencyMarketplaceMigrationLifecycleCollection : ICollectionFixture<AgencyMarketplaceMigrationLifecycleContainerFixture>;
