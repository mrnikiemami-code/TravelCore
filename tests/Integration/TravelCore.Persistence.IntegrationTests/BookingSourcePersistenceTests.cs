using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.AgencyMarketplace.Contracts;
using TravelCore.Modules.Booking.Contracts;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Booking.Infrastructure;
using TravelCore.Modules.Booking.Infrastructure.Services;
using Xunit;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(BookingMigrationLifecycleCollection))]
public sealed class BookingSourcePersistenceTests
{
    private readonly BookingMigrationLifecycleContainerFixture _postgres;

    public BookingSourcePersistenceTests(BookingMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Direct_And_Agency_Source_RoundTrip_Without_Peer_Fk_Or_Cloned_Agency_Tables()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var db = _postgres.CreateDbContext())
        {
            await BookingMigrator.MigrateAsync(db, ct);
        }

        var departure = new TourDepartureReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000801"));
        var profileId = Guid.Parse("0198b3e0-0000-7000-8000-000000000802");
        var offerId = Guid.Parse("0198b3e0-0000-7000-8000-000000000803");
        var now = Instant.FromUtc(2026, 8, 18, 6, 0);
        BookingId directId;
        BookingId agencyId;

        await using (var db = _postgres.CreateDbContext())
        {
            var direct = BookingAggregate.Create(departure, now);
            var agency = BookingAggregate.Create(
                departure,
                now,
                BookingSourceContext.ForAgency(
                    new AgencyProfileReference(profileId),
                    new AgencyOfferReference(offerId)));
            db.Bookings.AddRange(direct, agency);
            await db.SaveChangesAsync(ct);
            directId = direct.Id;
            agencyId = agency.Id;
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var direct = await db.Bookings.SingleAsync(x => x.Id == directId, ct);
            Assert.Equal(BookingSourceKind.Direct, direct.Source.Kind);
            Assert.Null(direct.Source.AgencyProfile);
            Assert.Null(direct.Source.AgencyOffer);
            Assert.Equal(BookingStatus.Pending, direct.Status);

            var agency = await db.Bookings.SingleAsync(x => x.Id == agencyId, ct);
            Assert.Equal(BookingSourceKind.Agency, agency.Source.Kind);
            Assert.Equal(profileId, agency.Source.AgencyProfile!.Value.AgencyProfileId);
            Assert.Equal(offerId, agency.Source.AgencyOffer!.Value.AgencyOfferId);
            Assert.Equal(BookingStatus.Pending, agency.Status);

            var conn = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.columns
                WHERE table_schema = 'booking'
                  AND table_name = 'bookings'
                  AND column_name IN ('source_kind', 'agency_profile_id', 'agency_offer_id');
                """;
            Assert.Equal(3, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));

            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.tables
                WHERE table_schema = 'booking'
                  AND table_name IN ('agency_profiles', 'agency_offers', 'commissions', 'settlements');
                """;
            Assert.Equal(0, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));

            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints tc
                JOIN information_schema.constraint_column_usage ccu
                  ON tc.constraint_schema = ccu.constraint_schema
                 AND tc.constraint_name = ccu.constraint_name
                WHERE tc.table_schema = 'booking'
                  AND tc.constraint_type = 'FOREIGN KEY'
                  AND ccu.table_schema IN ('agency_marketplace', 'tour', 'pricing', 'party', 'payment');
                """;
            Assert.Equal(0, Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)));
        }
    }

    [Fact]
    public async Task Creation_Service_Validates_Trusted_Agency_Context_Without_Calling_Direct_Origin()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var db = _postgres.CreateDbContext())
        {
            await BookingMigrator.MigrateAsync(db, ct);
        }

        var departure = new TourDepartureReference(Guid.Parse("0198b3e0-0000-7000-8000-000000000811"));
        var profileId = Guid.Parse("0198b3e0-0000-7000-8000-000000000812");
        var offerId = Guid.Parse("0198b3e0-0000-7000-8000-000000000813");
        var now = Instant.FromUtc(2026, 8, 18, 7, 0);
        var query = new FakeAgencyOriginQuery(
            new AgencyOriginProfileFacts(profileId, "Draft"),
            new AgencyOriginOfferFacts(offerId, profileId, Guid.CreateVersion7(), departure.LogicalId));

        BookingId directId;
        BookingId agencyId;
        await using (var db = _postgres.CreateDbContext())
        {
            var service = new BookingCreationService(db, query);
            directId = await service.CreateDirectAsync(departure, now, ct);
            Assert.Equal(0, query.ProfileCalls);
            Assert.Equal(0, query.OfferCalls);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => new BookingCreationService(db, new FakeAgencyOriginQuery(null, null))
                    .CreateAgencyAsync(departure, now, profileId, offerId, ct));

            var mismatched = new FakeAgencyOriginQuery(
                new AgencyOriginProfileFacts(profileId, "Draft"),
                new AgencyOriginOfferFacts(
                    offerId,
                    Guid.Parse("0198b3e0-0000-7000-8000-000000000899"),
                    Guid.CreateVersion7(),
                    null));
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => new BookingCreationService(db, mismatched)
                    .CreateAgencyAsync(departure, now, profileId, offerId, ct));

            agencyId = await service.CreateAgencyAsync(departure, now, profileId, offerId, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var direct = await db.Bookings.SingleAsync(x => x.Id == directId, ct);
            Assert.Equal(BookingSourceKind.Direct, direct.Source.Kind);
            var agency = await db.Bookings.SingleAsync(x => x.Id == agencyId, ct);
            Assert.Equal(BookingSourceKind.Agency, agency.Source.Kind);
            Assert.Equal(profileId, agency.Source.AgencyProfile!.Value.AgencyProfileId);
            Assert.Equal(offerId, agency.Source.AgencyOffer!.Value.AgencyOfferId);
            Assert.Equal(BookingStatus.Pending, agency.Status);
        }
    }

    private sealed class FakeAgencyOriginQuery : IAgencyOriginContextQuery
    {
        private readonly AgencyOriginProfileFacts? _profile;
        private readonly AgencyOriginOfferFacts? _offer;

        public FakeAgencyOriginQuery(AgencyOriginProfileFacts? profile, AgencyOriginOfferFacts? offer)
        {
            _profile = profile;
            _offer = offer;
        }

        public int ProfileCalls { get; private set; }
        public int OfferCalls { get; private set; }

        public Task<AgencyOriginProfileFacts?> GetProfileAsync(
            Guid agencyProfileId,
            CancellationToken cancellationToken = default)
        {
            ProfileCalls++;
            return Task.FromResult(
                _profile is { AgencyProfileId: var id } && id == agencyProfileId ? _profile : null);
        }

        public Task<AgencyOriginOfferFacts?> GetOfferAsync(
            Guid agencyOfferId,
            CancellationToken cancellationToken = default)
        {
            OfferCalls++;
            return Task.FromResult(
                _offer is { AgencyOfferId: var id } && id == agencyOfferId ? _offer : null);
        }
    }
}
