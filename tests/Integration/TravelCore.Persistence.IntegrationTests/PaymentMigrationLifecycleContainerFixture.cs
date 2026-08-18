using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Payment.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for Payment module migration lifecycle (TC-P20-T001).
/// </summary>
public sealed class PaymentMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_payment_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public PaymentDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: PaymentDbContext.SchemaName)
            .Options;
        return new PaymentDbContext(options);
    }
}

[CollectionDefinition(nameof(PaymentMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class PaymentMigrationLifecycleCollection : ICollectionFixture<PaymentMigrationLifecycleContainerFixture>;
