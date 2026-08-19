using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Flight.Contracts;
using TravelCore.Modules.Flight.Domain;
using TravelCore.Modules.Flight.Infrastructure;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

/// <summary>
/// Real-PostgreSQL FlightBooking itinerary persistence (TC-P22-T002). Same-schema FKs only.
/// </summary>
[Collection(nameof(FlightMigrationLifecycleCollection))]
public sealed class FlightMigrationLifecycleTests
{
    private readonly FlightMigrationLifecycleContainerFixture _postgres;

    public FlightMigrationLifecycleTests(FlightMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task FlightMigrationLifecycle_Apply_Itinerary_Tables_And_RoundTrip()
    {
        var ct = TestContext.Current.CancellationToken;
        string[] expectedMigrations;

        await using (var inventoryDb = _postgres.CreateDbContext())
        {
            expectedMigrations = inventoryDb.Database.GetMigrations().ToArray();
            Assert.Equal(7, expectedMigrations.Length);
            Assert.EndsWith("_InitialFlightScaffolding", expectedMigrations[0], StringComparison.Ordinal);
            Assert.EndsWith("_AddFlightBookingItinerary", expectedMigrations[1], StringComparison.Ordinal);
            Assert.EndsWith("_AddFlightOfferSnapshots", expectedMigrations[2], StringComparison.Ordinal);
            Assert.EndsWith("_AddFlightSupplierReservations", expectedMigrations[3], StringComparison.Ordinal);
            Assert.EndsWith("_AddFlightPaymentAndTicketing", expectedMigrations[4], StringComparison.Ordinal);
            Assert.EndsWith("_AddFlightBookingCancellation", expectedMigrations[5], StringComparison.Ordinal);
            Assert.EndsWith("_AddPublicFlightBookingAccessAndIdempotency", expectedMigrations[6], StringComparison.Ordinal);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            await FlightMigrator.MigrateAsync(db, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);

            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int FROM pg_namespace WHERE nspname = 'flight';
                """, ct));
            Assert.Equal(1, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'flight'
                  AND table_name = '__EFMigrationsHistory';
                """, ct));
            Assert.Equal(27, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'flight'
                  AND table_name NOT IN ('__EFMigrationsHistory');
                """, ct));
            foreach (var table in new[]
                     {
                         "flight_bookings", "flight_journeys", "flight_segments", "flight_passengers",
                         "flight_offer_snapshots", "flight_booking_monetary_snapshots",
                         "flight_passenger_category_fare_snapshots", "flight_fare_rule_snapshots",
                         "flight_baggage_allowance_snapshots", "flight_offer_idempotency",
                         "flight_supplier_reservations", "flight_supplier_reservation_attempts",
                         "flight_supplier_reservation_idempotency", "flight_reconciliation_issues",
                         "flight_tickets", "flight_ticketing_attempts", "flight_ticketing_idempotency",
                         "flight_booking_payment_evidence", "flight_booking_payment_compensation_evidence",
                         "flight_payment_success_inbox", "flight_refund_success_inbox", "outbox_messages",
                         "flight_booking_cancellations", "flight_supplier_reversal_attempts",
                         "flight_booking_cancellation_idempotency",
                         "flight_booking_access_credentials", "flight_booking_public_idempotency",
                     })
            {
                Assert.Equal(1, await ScalarIntAsync(conn, $"""
                    SELECT COUNT(*)::int
                    FROM information_schema.tables
                    WHERE table_schema = 'flight'
                      AND table_name = '{table}';
                    """, ct));
            }

            Assert.Equal(0, await ScalarIntAsync(conn, """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.constraint_type = 'FOREIGN KEY'
                  AND tc.table_schema = 'flight'
                  AND ccu.table_schema <> 'flight';
                """, ct));

            Assert.False(db.Database.HasPendingModelChanges());
            Assert.Empty(await db.Database.GetPendingMigrationsAsync(ct));
        }

        var outboundDep = Instant.FromUtc(2026, 9, 1, 6, 0);
        var outboundArr = Instant.FromUtc(2026, 9, 1, 10, 0);
        var inboundDep = Instant.FromUtc(2026, 9, 8, 8, 0);
        var inboundArr = Instant.FromUtc(2026, 9, 8, 14, 0);
        var created = FlightBooking.Create(
            FlightTripType.RoundTrip,
            [
                new FlightJourneySpecification(
                [
                    new FlightSegmentSpecification(
                        new AirportReference("THR"),
                        new AirportReference("LHR"),
                        outboundDep,
                        "Asia/Tehran",
                        outboundArr,
                        "Europe/London",
                        new AirlineReference("TK"),
                        new AirlineReference("BA"),
                        "TK800"),
                ]),
                new FlightJourneySpecification(
                [
                    new FlightSegmentSpecification(
                        new AirportReference("LHR"),
                        new AirportReference("THR"),
                        inboundDep,
                        "Europe/London",
                        inboundArr,
                        "Asia/Tehran",
                        new AirlineReference("TK"),
                        null,
                        "TK801"),
                ]),
            ],
            [
                new FlightPassengerSpecification("Ada", "Lovelace", FlightPassengerCategory.Adult),
                new FlightPassengerSpecification("Alan", "Turing", FlightPassengerCategory.Child),
            ]);

        await using (var db = _postgres.CreateDbContext())
        {
            db.FlightBookings.Add(created);
            await db.SaveChangesAsync(ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.FlightBookings
                .Include(b => b.Journeys)
                    .ThenInclude(j => j.Segments)
                .Include(b => b.Passengers)
                .SingleAsync(b => b.Id == created.Id, ct);

            Assert.Equal(FlightTripType.RoundTrip, loaded.TripType);
            Assert.Equal(2, loaded.JourneyCount);
            Assert.Equal("THR", loaded.Outbound.Origin.IataCode);
            Assert.Equal("LHR", loaded.Outbound.Destination.IataCode);
            Assert.Equal("TK", loaded.Outbound.Segments[0].MarketingCarrier.IataCode);
            Assert.Equal("BA", loaded.Outbound.Segments.OrderBy(s => s.Ordinal).First().OperatingCarrier!.Value.IataCode);
            Assert.Equal("TK800", loaded.Outbound.Segments[0].FlightNumber);
            Assert.Equal(outboundDep, loaded.Outbound.Segments[0].DepartureAt);
            Assert.Equal("Asia/Tehran", loaded.Outbound.Segments[0].DepartureTimeZoneId);
            Assert.Equal(["Ada", "Alan"], loaded.Passengers.OrderBy(p => p.Ordinal).Select(p => p.GivenName).ToArray());
            Assert.False(db.Database.HasPendingModelChanges());
        }
    }

    private static async Task<int> ScalarIntAsync(DbConnection conn, string sql, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }
}
