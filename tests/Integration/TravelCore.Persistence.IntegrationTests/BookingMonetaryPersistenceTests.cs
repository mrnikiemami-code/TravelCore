using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Booking.Infrastructure;
using TravelCore.Modules.Booking.Infrastructure.Services;
using TravelCore.Modules.Pricing.Contracts;
using Xunit;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(BookingMigrationLifecycleCollection))]
public sealed class BookingMonetaryPersistenceTests
{
    private readonly BookingMigrationLifecycleContainerFixture _postgres;

    public BookingMonetaryPersistenceTests(BookingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Monetary_Snapshot_RoundTrips_Money_And_Has_No_Pricing_Fk()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var db = _postgres.CreateDbContext())
        {
            await BookingMigrator.MigrateAsync(db, ct);
        }

        var departure = new TourDepartureReference(Guid.CreateVersion7());
        var quoteId = Guid.CreateVersion7();
        var now = Instant.FromUtc(2026, 8, 18, 13, 0);
        BookingId id;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = BookingAggregate.Create(departure, now);
            booking.AcceptQuote(Facts(quoteId, departure.LogicalId, now), now);
            db.Bookings.Add(booking);
            await db.SaveChangesAsync(ct);
            id = booking.Id;
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.Bookings
                .Include(x => x.MonetarySnapshot)
                .ThenInclude(x => x!.Components)
                .SingleAsync(x => x.Id == id, ct);
            Assert.NotNull(loaded.MonetarySnapshot);
            Assert.Equal(quoteId, loaded.MonetarySnapshot.QuoteReference.LogicalId);
            Assert.Equal(110.12345678m, loaded.MonetarySnapshot.Total.Amount);
            Assert.Equal("USD", loaded.MonetarySnapshot.Total.Currency.Value);
            Assert.Equal(2, loaded.MonetarySnapshot.Components.Count);
            Assert.Contains(loaded.MonetarySnapshot.Components, c => c.Kind == BookingMonetaryComponentKind.Base);
            Assert.Equal(BookingStatus.Pending, loaded.Status);

            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = 'booking'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema IN ('pricing', 'tour', 'payment');
                """;
            Assert.Equal(0, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));

            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'booking'
                  AND column_name IN ('tax_rate', 'discount_rule', 'fx_rate', 'agency_markup');
                """;
            Assert.Equal(0, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));

            cmd.CommandText = $"""
                SELECT COUNT(*)::int
                FROM booking.booking_monetary_snapshots
                WHERE booking_id = '{id.Value}';
                """;
            Assert.Equal(1, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));
        }
    }

    [Fact]
    public async Task Service_Accepts_Trusted_Quote_And_Rejects_Missing_Or_Different_Quote()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var db = _postgres.CreateDbContext())
        {
            await BookingMigrator.MigrateAsync(db, ct);
        }

        var departure = new TourDepartureReference(Guid.CreateVersion7());
        var quoteId = Guid.CreateVersion7();
        var otherQuoteId = Guid.CreateVersion7();
        var now = Instant.FromUtc(2026, 8, 18, 14, 0);
        BookingId id;
        await using (var db = _postgres.CreateDbContext())
        {
            var booking = BookingAggregate.Create(departure, now);
            db.Bookings.Add(booking);
            await db.SaveChangesAsync(ct);
            id = booking.Id;
        }

        var query = new FakeAuthoritativeQuoteQuery(quoteId, departure.LogicalId, now);
        await using (var db = _postgres.CreateDbContext())
        {
            var service = new BookingQuoteService(db, query);
            await service.AcceptQuoteAsync(id, quoteId, now, ct);
            await service.AcceptQuoteAsync(id, quoteId, now.Plus(Duration.FromMinutes(1)), ct);
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AcceptQuoteAsync(id, Guid.CreateVersion7(), now, ct));
            query.OtherId = otherQuoteId;
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AcceptQuoteAsync(id, otherQuoteId, now, ct));
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var loaded = await db.Bookings
                .Include(x => x.MonetarySnapshot)
                .ThenInclude(x => x!.Components)
                .SingleAsync(x => x.Id == id, ct);
            Assert.Equal(quoteId, loaded.MonetarySnapshot!.QuoteReference.LogicalId);
            Assert.Single(loaded.MonetarySnapshot.Components);
        }
    }

    private static AuthoritativeQuoteFacts Facts(Guid quoteId, Guid departureId, Instant now)
    {
        return AuthoritativeQuoteFacts.Create(
            PricingQuoteReference.From(quoteId),
            Guid.CreateVersion7(),
            BookingOwnershipBoundary.InitialTarget,
            departureId,
            now.Minus(Duration.FromMinutes(10)),
            now.Plus(Duration.FromHours(6)),
            [
                new AuthoritativeQuoteComponentFact(
                    BookingMonetaryComponentKind.Base,
                    new TravelCore.Money.Money(100.12345678m, "USD"),
                    0,
                    "BASE",
                    "Base"),
                new AuthoritativeQuoteComponentFact(
                    BookingMonetaryComponentKind.Fee,
                    new TravelCore.Money.Money(10m, "USD"),
                    1,
                    "FEE",
                    "Fee")
            ]);
    }

    private sealed class FakeAuthoritativeQuoteQuery : IAuthoritativeQuoteQuery
    {
        private readonly Guid _quoteId;
        private readonly Guid _departureId;
        private readonly Instant _now;

        public FakeAuthoritativeQuoteQuery(Guid quoteId, Guid departureId, Instant now)
        {
            _quoteId = quoteId;
            _departureId = departureId;
            _now = now;
        }

        public Guid? OtherId { get; set; }

        public Task<AuthoritativeQuote?> GetByIdAsync(Guid quoteId, CancellationToken cancellationToken = default)
        {
            if (quoteId != _quoteId && quoteId != OtherId)
            {
                return Task.FromResult<AuthoritativeQuote?>(null);
            }

            var created = _now.Minus(Duration.FromMinutes(10)).ToDateTimeOffset();
            var expires = _now.Plus(Duration.FromHours(6)).ToDateTimeOffset();
            return Task.FromResult<AuthoritativeQuote?>(new AuthoritativeQuote(
                quoteId,
                Guid.CreateVersion7(),
                BookingOwnershipBoundary.InitialTarget,
                _departureId,
                created,
                expires,
                "EUR",
                50m,
                [
                    new AuthoritativeQuoteComponent(
                        "Base",
                        new MoneyResponse(50m, "EUR"),
                        0,
                        "BASE",
                        "Base")
                ]));
        }
    }
}
