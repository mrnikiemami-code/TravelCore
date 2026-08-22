using TravelCore.Modules.AgencyMarketplace.Domain;
using TravelCore.Modules.AgencyMarketplace.Infrastructure.Policies;
using TravelCore.Modules.AgencyMarketplace.Infrastructure.Services;
using TravelCore.Modules.AgencyMarketplace.Contracts;
using Xunit;

namespace TravelCore.Modules.AgencyMarketplace.UnitTests;

/// <summary>
/// Admin governance boundaries for AgencyOffer (TC-P38-T010).
/// </summary>
public sealed class AgencyOfferGovernanceTests
{
    private static AgencyProfileId ProfileA() =>
        AgencyProfileId.From(Guid.Parse("0198b3e0-0000-7000-8000-0000000000a1"));

    private static AgencyProfileId ProfileB() =>
        AgencyProfileId.From(Guid.Parse("0198b3e0-0000-7000-8000-0000000000a2"));

    private static Guid Tour() => Guid.Parse("0198b3e0-0000-7000-8000-0000000000dd");

    [Fact]
    public void Self_moderation_is_rejected_for_owning_agency()
    {
        var offer = AgencyOffer.Create(ProfileA(), Tour());
        offer.Submit();

        var ex = Assert.Throws<UnauthorizedAccessException>(() =>
            AgencyOfferGovernanceService.EnsureNotSelfModeration(offer, ProfileA().Value));
        Assert.Contains("own AgencyOffer", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Other_agency_or_pure_admin_may_moderate()
    {
        var offer = AgencyOffer.Create(ProfileA(), Tour());
        offer.Submit();

        AgencyOfferGovernanceService.EnsureNotSelfModeration(offer, ProfileB().Value);
        AgencyOfferGovernanceService.EnsureNotSelfModeration(offer, actingAgencyProfileId: null);
    }

    [Fact]
    public void Suspend_requires_Published_and_unlists()
    {
        var offer = AgencyOffer.Create(ProfileA(), Tour());
        offer.Submit();
        offer.Approve();
        offer.Publish();

        offer.Suspend();
        Assert.Equal(AgencyOfferPublicationStatus.Suspended, offer.PublicationStatus);
        Assert.Equal(AgencyOfferVisibility.Unlisted, offer.Visibility);
        Assert.False(offer.SalesAvailability.SalesOpen);
    }

    [Fact]
    public async Task Default_policy_stubs_allow_without_commission_fields()
    {
        var context = new AgencyOfferPolicyContext(
            OfferId: Guid.Parse("0198b3e0-0000-7000-8000-0000000000e1"),
            AgencyProfileId: ProfileA().Value,
            TourProductId: Tour(),
            SalesChannel: "Public",
            PublicationStatus: "Submitted",
            Visibility: "Unlisted",
            OfferStatus: "Draft");

        await new AllowAgencyOfferCommercialPolicy().EnsureAllowsAsync(context, TestContext.Current.CancellationToken);
        await new AllowAgencyOfferContentPolicy().EnsureAllowsAsync(context, TestContext.Current.CancellationToken);
        await new AllowAgencyOfferChannelPolicy().EnsureAllowsAsync(context, TestContext.Current.CancellationToken);

        Assert.Null(typeof(AgencyOfferPolicyContext).GetProperty("Commission"));
        Assert.Null(typeof(AgencyOfferPolicyContext).GetProperty("Settlement"));
        Assert.Null(typeof(AgencyOfferPolicyContext).GetProperty("Payout"));
    }
}
