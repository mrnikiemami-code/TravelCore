using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.HotelBooking.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Dedicated clean PostgreSQL for HotelBooking module migration lifecycle (TC-P21-T001).
/// </summary>
public sealed class HotelBookingMigrationLifecycleContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_hotel_booking_lifecycle")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public HotelBookingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<HotelBookingDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: HotelBookingDbContext.SchemaName)
            .Options;
        return new HotelBookingDbContext(options);
    }
}

[CollectionDefinition(nameof(HotelBookingMigrationLifecycleCollection), DisableParallelization = true)]
public sealed class HotelBookingMigrationLifecycleCollection : ICollectionFixture<HotelBookingMigrationLifecycleContainerFixture>;
