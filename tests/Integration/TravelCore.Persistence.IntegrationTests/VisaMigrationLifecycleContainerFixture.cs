using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Visa.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for Visa module migration lifecycle (TC-P17-T001).
/// </summary>
public sealed class VisaMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_visa_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public VisaDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<VisaDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: VisaDbContext.SchemaName)
            .Options;
        return new VisaDbContext(options);
    }
}

[CollectionDefinition(nameof(VisaMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class VisaMigrationLifecycleCollection : ICollectionFixture<VisaMigrationLifecycleContainerFixture>;
