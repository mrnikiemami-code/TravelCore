using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.CommercialFinance.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for Commercial Finance module migration lifecycle (TC-P39-T006).
/// </summary>
public sealed class CommercialFinanceMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_commercial_finance_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public CommercialFinanceDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CommercialFinanceDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: CommercialFinanceDbContext.SchemaName)
            .Options;
        return new CommercialFinanceDbContext(options);
    }
}

[CollectionDefinition(nameof(CommercialFinanceMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class CommercialFinanceMigrationLifecycleCollection
    : ICollectionFixture<CommercialFinanceMigrationLifecycleContainerFixture>;
