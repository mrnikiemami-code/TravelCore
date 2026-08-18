using TravelCore.Modules.AgencyMarketplace.Domain;
using TravelCore.Modules.AgencyMarketplace.Infrastructure;
using TravelCore.Modules.AgencyMarketplace.Infrastructure.Services;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(AgencyMarketplaceMigrationLifecycleCollection))]
public sealed class AgencyOriginContextQueryPersistenceTests
{
    private readonly AgencyMarketplaceMigrationLifecycleContainerFixture _postgres;

    public AgencyOriginContextQueryPersistenceTests(AgencyMarketplaceMigrationLifecycleContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Origin_Query_Returns_Logical_Profile_And_Offer_Facts_Without_Peer_Types()
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var db = _postgres.CreateDbContext())
        {
            await AgencyMarketplaceMigrator.MigrateAsync(db, ct);
        }

        var partyId = MarketplacePartyId.From(Guid.CreateVersion7());
        var tourProductId = Guid.CreateVersion7();
        var departureId = Guid.CreateVersion7();
        AgencyProfileId profileId;
        AgencyOfferId offerId;

        await using (var db = _postgres.CreateDbContext())
        {
            var profile = AgencyProfile.Create(partyId, new AgencyDisplayInfo("Origin Agency", null, null));
            var offer = AgencyOffer.Create(profile.Id, tourProductId);
            offer.SetReferencedTourDeparture(MarketplaceTourDepartureId.From(departureId));
            db.AgencyProfiles.Add(profile);
            db.AgencyOffers.Add(offer);
            await db.SaveChangesAsync(ct);
            profileId = profile.Id;
            offerId = offer.Id;
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var query = new AgencyOriginContextQuery(db);
            var profile = await query.GetProfileAsync(profileId.Value, ct);
            Assert.NotNull(profile);
            Assert.Equal(profileId.Value, profile.AgencyProfileId);
            Assert.Equal(nameof(AgencyProfileStatus.Draft), profile.Status);

            var offer = await query.GetOfferAsync(offerId.Value, ct);
            Assert.NotNull(offer);
            Assert.Equal(offerId.Value, offer.AgencyOfferId);
            Assert.Equal(profileId.Value, offer.AgencyProfileId);
            Assert.Equal(tourProductId, offer.TourProductId);
            Assert.Equal(departureId, offer.ReferencedTourDepartureId);
            Assert.Null(await query.GetProfileAsync(Guid.CreateVersion7(), ct));
            Assert.Null(await query.GetOfferAsync(Guid.CreateVersion7(), ct));
        }
    }
}
