using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Booking.Infrastructure;
using TravelCore.Modules.Payment.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Shared PostgreSQL for Booking+Payment compensation E2E (TC-P20-T006).
/// </summary>
public sealed class PaymentBookingCompensationContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_p20_compensation")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public BookingDbContext CreateBookingDbContext()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: BookingDbContext.SchemaName)
            .Options;
        return new BookingDbContext(options);
    }

    public PaymentDbContext CreatePaymentDbContext()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: PaymentDbContext.SchemaName)
            .Options;
        return new PaymentDbContext(options);
    }
}

[CollectionDefinition(nameof(PaymentBookingCompensationCollection), DisableParallelization = true)]
public sealed class PaymentBookingCompensationCollection : ICollectionFixture<PaymentBookingCompensationContainerFixture>;
