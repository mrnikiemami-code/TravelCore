using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Booking.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for Booking module migration lifecycle (TC-P19-T001).
/// </summary>
public sealed class BookingMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_booking_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public BookingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: BookingDbContext.SchemaName)
            .Options;
        return new BookingDbContext(options);
    }
}

[CollectionDefinition(nameof(BookingMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class BookingMigrationLifecycleCollection : ICollectionFixture<BookingMigrationLifecycleContainerFixture>;
