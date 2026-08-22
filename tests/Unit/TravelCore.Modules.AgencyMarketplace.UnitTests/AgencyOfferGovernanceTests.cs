using TravelCore.Modules.AgencyMarketplace.Contracts;
using TravelCore.Modules.AgencyMarketplace.Domain;
using TravelCore.Modules.AgencyMarketplace.Infrastructure.Policies;
using TravelCore.Modules.AgencyMarketplace.Infrastructure.Services;
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

    [Theory]
    [InlineData(null, AgencyOfferPublicationStatus.Submitted)]
    [InlineData("pending", AgencyOfferPublicationStatus.Submitted)]
    [InlineData("Submitted", AgencyOfferPublicationStatus.Submitted)]
    [InlineData("Approved", AgencyOfferPublicationStatus.Approved)]
    [InlineData("Rejected", AgencyOfferPublicationStatus.Rejected)]
    [InlineData("Suspended", AgencyOfferPublicationStatus.Suspended)]
    [InlineData("Retired", AgencyOfferPublicationStatus.Retired)]
    public void Ops_status_filter_parses_allowed_values(
        string? raw,
        AgencyOfferPublicationStatus expected)
    {
        Assert.Equal(expected, AgencyOfferGovernanceService.ParseOpsPublicationStatus(raw));
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("Published")]
    [InlineData("Archived")]
    [InlineData("money")]
    public void Ops_status_filter_rejects_non_ops_values(string raw)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AgencyOfferGovernanceService.ParseOpsPublicationStatus(raw));
        Assert.Equal("publicationStatus", ex.ParamName);
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
    public async Task Default_policy_stubs_allow_without_money_fields()
    {
        var ct = TestContext.Current.CancellationToken;
        var context = new AgencyOfferPolicyContext(
            OfferId: Guid.Parse("0198b3e0-0000-7000-8000-0000000000e1"),
            AgencyProfileId: ProfileA().Value,
            TourProductId: Tour(),
            SalesChannel: "Public",
            PublicationStatus: "Submitted",
            Visibility: "Unlisted",
            OfferStatus: "Draft");

        var commercial = await new AllowAgencyOfferCommercialPolicy().EvaluateAsync(context, ct);
        var content = await new AllowAgencyOfferContentPolicy().EvaluateAsync(context, ct);
        var channel = await new AllowAgencyOfferChannelPolicy().EvaluateAsync(context, ct);

        Assert.True(commercial.IsAllowed);
        Assert.True(content.IsAllowed);
        Assert.True(channel.IsAllowed);
        Assert.Null(typeof(AgencyOfferPolicyContext).GetProperty("Commission"));
        Assert.Null(typeof(AgencyOfferPolicyContext).GetProperty("Settlement"));
        Assert.Null(typeof(AgencyOfferPolicyContext).GetProperty("Payout"));
    }
}
