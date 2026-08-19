using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using TravelCore.Modules.Payment.Infrastructure;
using TravelCore.Modules.Payment.Infrastructure.Services;
using Xunit;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(PaymentMigrationLifecycleCollection))]
public sealed class PaymentHotelTargetPersistenceTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 18, 18, 0);
    private readonly PaymentMigrationLifecycleContainerFixture _postgres;

    public PaymentHotelTargetPersistenceTests(PaymentMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Hotel_Payment_RoundTrips_With_Nullable_Tour_Column_And_Filtered_Uniques()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await PaymentMigrator.MigrateAsync(migrate, ct);
        }

        var hotel = new HotelBookingPaymentReference(Guid.CreateVersion7());
        var tour = new BookingReference(Guid.CreateVersion7());
        PaymentId hotelPaymentId;
        PaymentId tourPaymentId;
        await using (var db = _postgres.CreateDbContext())
        {
            var hotelPayment = PaymentAggregate.CreateForHotel(hotel, Now);
            hotelPayment.BindExecutionSnapshot(Guid.CreateVersion7(), new MoneyValue(1_000_000m, "IRR"), Now);
            db.Payments.Add(hotelPayment);
            var tourPayment = PaymentAggregate.Create(tour, Now);
            db.Payments.Add(tourPayment);
            await db.SaveChangesAsync(ct);
            hotelPaymentId = hotelPayment.Id;
            tourPaymentId = tourPayment.Id;
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loadedHotel = await db.Payments.SingleAsync(x => x.Id == hotelPaymentId, ct);
            Assert.Equal(PaymentTargetKind.HotelBooking, loadedHotel.TargetKind);
            Assert.Equal(hotel, loadedHotel.HotelBooking);
            Assert.Null(loadedHotel.Booking);
            Assert.Equal(hotel.HotelBookingId, loadedHotel.TargetReferenceId);

            var loadedTour = await db.Payments.SingleAsync(x => x.Id == tourPaymentId, ct);
            Assert.Equal(PaymentTargetKind.TourBooking, loadedTour.TargetKind);
            Assert.Equal(tour, loadedTour.Booking);
            Assert.Null(loadedTour.HotelBooking);

            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"""
                SELECT COUNT(*)::int
                FROM payment.payments
                WHERE id = '{hotelPaymentId.Value}'
                  AND booking_id IS NULL
                  AND hotel_booking_id = '{hotel.HotelBookingId}';
                """;
            Assert.Equal(1, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));

            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'payment'
                  AND table_name = 'payments'
                  AND column_name = 'target_type';
                """;
            Assert.Equal(0, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));

            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints
                WHERE table_schema = 'payment'
                  AND constraint_name IN ('ck_payments_exactly_one_target', 'ck_refunds_exactly_one_target');
                """;
            Assert.Equal(2, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));

            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM pg_indexes
                WHERE schemaname = 'payment'
                  AND indexname IN ('ux_payments_booking_id', 'ux_payments_hotel_booking_id', 'ux_payments_flight_booking_id');
                """;
            Assert.Equal(3, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));

            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_name = ccu.constraint_name
                 AND tc.table_schema = ccu.table_schema
                WHERE tc.constraint_type = 'FOREIGN KEY'
                  AND tc.table_schema = 'payment'
                  AND ccu.table_schema = 'hotel_booking';
                """;
            Assert.Equal(0, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));
        }
    }

    [Fact]
    public async Task Duplicate_HotelBooking_GetOrCreate_And_Exactly_One_Target_Check()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var migrate = _postgres.CreateDbContext())
        {
            await PaymentMigrator.MigrateAsync(migrate, ct);
        }

        var hotel = new HotelBookingPaymentReference(Guid.CreateVersion7());
        PaymentId firstId;
        await using (var db = _postgres.CreateDbContext())
        {
            var service = new PaymentGetOrCreateService(db, new FixedClock(Now));
            firstId = (await service.GetOrCreateAsync(hotel, ct)).Id;
            var second = await service.GetOrCreateAsync(hotel, ct);
            Assert.Equal(firstId, second.Id);
            Assert.Equal(1, await db.Payments.CountAsync(x => x.HotelBooking == hotel, ct));
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var duplicate = await Record.ExceptionAsync(() => db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO payment.payments
                    (id, booking_id, hotel_booking_id, status, created_at, status_changed_at, version)
                VALUES
                    ({0}, NULL, {1}, 1, TIMESTAMPTZ '2026-08-18 18:00:00+00', TIMESTAMPTZ '2026-08-18 18:00:00+00', 0);
                """,
                Guid.CreateVersion7(),
                hotel.HotelBookingId));
            Assert.NotNull(duplicate);
            Assert.Contains("ux_payments_hotel_booking_id", duplicate.Message, StringComparison.OrdinalIgnoreCase);

            var bothTargets = await Record.ExceptionAsync(() => db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO payment.payments
                    (id, booking_id, hotel_booking_id, status, created_at, status_changed_at, version)
                VALUES
                    ({0}, {1}, {2}, 1, TIMESTAMPTZ '2026-08-18 18:00:00+00', TIMESTAMPTZ '2026-08-18 18:00:00+00', 0);
                """,
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7()));
            Assert.NotNull(bothTargets);
            Assert.Contains("ck_payments_exactly_one_target", bothTargets.Message, StringComparison.OrdinalIgnoreCase);

            var neither = await Record.ExceptionAsync(() => db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO payment.payments
                    (id, booking_id, hotel_booking_id, status, created_at, status_changed_at, version)
                VALUES
                    ({0}, NULL, NULL, 1, TIMESTAMPTZ '2026-08-18 18:00:00+00', TIMESTAMPTZ '2026-08-18 18:00:00+00', 0);
                """,
                Guid.CreateVersion7()));
            Assert.NotNull(neither);
            Assert.Contains("ck_payments_exactly_one_target", neither.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    private sealed class FixedClock(Instant now) : IClock
    {
        public Instant GetCurrentInstant() => now;
    }
}
