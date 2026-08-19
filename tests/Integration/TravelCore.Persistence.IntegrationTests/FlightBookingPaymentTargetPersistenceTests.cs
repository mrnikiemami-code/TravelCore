using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Flight.Infrastructure;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure;
using TravelCore.Modules.Payment.Infrastructure.Services;
using Xunit;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(PaymentMigrationLifecycleCollection))]
public sealed class FlightBookingPaymentTargetPersistenceTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 18, 0);
    private readonly PaymentMigrationLifecycleContainerFixture _postgres;

    public FlightBookingPaymentTargetPersistenceTests(PaymentMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task One_FlightBooking_Has_One_Payment_And_Exactly_One_Target()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await PaymentMigrator.MigrateAsync(migrate, ct);
        }

        var flight = new FlightBookingPaymentReference(Guid.CreateVersion7());
        PaymentId paymentId;
        await using (var db = _postgres.CreateDbContext())
        {
            var service = new PaymentGetOrCreateService(db, new FixedClock(Now));
            paymentId = (await service.GetOrCreateAsync(flight, ct)).Id;
            var second = await service.GetOrCreateAsync(flight, ct);
            Assert.Equal(paymentId, second.Id);
            Assert.Equal(PaymentTargetKind.FlightBooking, second.TargetKind);
            Assert.Equal(1, await db.Payments.CountAsync(x => x.FlightBooking == flight, ct));
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.Payments.SingleAsync(x => x.Id == paymentId, ct);
            Assert.Equal(flight, loaded.FlightBooking);
            Assert.Null(loaded.Booking);
            Assert.Null(loaded.HotelBooking);

            var duplicate = await Record.ExceptionAsync(() => db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO payment.payments
                    (id, booking_id, hotel_booking_id, flight_booking_id, status, created_at, status_changed_at, version)
                VALUES
                    ({0}, NULL, NULL, {1}, 1, TIMESTAMPTZ '2026-08-18 18:00:00+00', TIMESTAMPTZ '2026-08-18 18:00:00+00', 0);
                """,
                Guid.CreateVersion7(),
                flight.FlightBookingId));
            Assert.NotNull(duplicate);
            Assert.Contains("ux_payments_flight_booking_id", duplicate.Message, StringComparison.OrdinalIgnoreCase);

            var twoTargets = await Record.ExceptionAsync(() => db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO payment.payments
                    (id, booking_id, hotel_booking_id, flight_booking_id, status, created_at, status_changed_at, version)
                VALUES
                    ({0}, {1}, NULL, {2}, 1, TIMESTAMPTZ '2026-08-18 18:00:00+00', TIMESTAMPTZ '2026-08-18 18:00:00+00', 0);
                """,
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7()));
            Assert.NotNull(twoTargets);
            Assert.Contains("ck_payments_exactly_one_target", twoTargets.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    private sealed class FixedClock(Instant now) : IClock
    {
        public Instant GetCurrentInstant() => now;
    }
}
