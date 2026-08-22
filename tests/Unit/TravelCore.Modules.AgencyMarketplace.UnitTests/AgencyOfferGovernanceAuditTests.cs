using TravelCore.Modules.AgencyMarketplace.Domain;
using Xunit;

namespace TravelCore.Modules.AgencyMarketplace.UnitTests;

/// <summary>
/// Governance audit event foundation (TC-P38-T013).
/// </summary>
public sealed class AgencyOfferGovernanceAuditTests
{
    [Fact]
    public void Governance_event_captures_lifecycle_without_money_fields()
    {
        var offerId = AgencyOfferId.From(Guid.Parse("0198b3e0-0000-7000-8000-0000000000e1"));
        var profileId = AgencyProfileId.From(Guid.Parse("0198b3e0-0000-7000-8000-0000000000a1"));

        var evt = AgencyOfferGovernanceEvent.Create(
            offerId,
            profileId,
            AgencyOfferGovernanceEventKind.Approved,
            actorKind: "Admin",
            actorAccountId: Guid.Parse("0198b3e0-0000-7000-8000-0000000000ac"),
            fromPublicationStatus: "Submitted",
            toPublicationStatus: "Approved");

        Assert.Equal(AgencyOfferGovernanceEventKind.Approved, evt.Kind);
        Assert.Equal("Admin", evt.ActorKind);
        Assert.Equal("Submitted", evt.FromPublicationStatus);
        Assert.Equal("Approved", evt.ToPublicationStatus);
        Assert.Null(typeof(AgencyOfferGovernanceEvent).GetProperty("CommissionAmount"));
        Assert.Null(typeof(AgencyOfferGovernanceEvent).GetProperty("SettlementId"));
        Assert.Null(typeof(AgencyOfferGovernanceEvent).GetProperty("PayoutId"));
    }

    [Fact]
    public void Policy_denied_event_stores_reason_codes()
    {
        var evt = AgencyOfferGovernanceEvent.Create(
            AgencyOfferId.From(Guid.Parse("0198b3e0-0000-7000-8000-0000000000e2")),
            AgencyProfileId.From(Guid.Parse("0198b3e0-0000-7000-8000-0000000000a1")),
            AgencyOfferGovernanceEventKind.PolicyDenied,
            actorKind: "Admin",
            policyCode: "CONTENT_DENY_TEST",
            policyName: "DenyContentPolicy",
            reason: "Denied for test.");

        Assert.Equal(AgencyOfferGovernanceEventKind.PolicyDenied, evt.Kind);
        Assert.Equal("CONTENT_DENY_TEST", evt.PolicyCode);
        Assert.Equal("DenyContentPolicy", evt.PolicyName);
    }
}
