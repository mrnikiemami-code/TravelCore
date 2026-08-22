using TravelCore.Modules.AgencyMarketplace.Contracts;
using TravelCore.Modules.AgencyMarketplace.Infrastructure.Policies;
using Xunit;

namespace TravelCore.Modules.AgencyMarketplace.UnitTests;

/// <summary>
/// AgencyOffer policy decision foundation (TC-P38-T011).
/// </summary>
public sealed class AgencyOfferPolicyFoundationTests
{
    private static AgencyOfferPolicyContext SampleContext() =>
        new(
            OfferId: Guid.Parse("0198b3e0-0000-7000-8000-0000000000e1"),
            AgencyProfileId: Guid.Parse("0198b3e0-0000-7000-8000-0000000000a1"),
            TourProductId: Guid.Parse("0198b3e0-0000-7000-8000-0000000000dd"),
            SalesChannel: "Public",
            PublicationStatus: "Submitted",
            Visibility: "Unlisted",
            OfferStatus: "Draft");

    [Fact]
    public async Task Default_composite_evaluator_allows()
    {
        var ct = TestContext.Current.CancellationToken;
        var evaluator = new AgencyOfferPolicyEvaluator(
            new AllowAgencyOfferCommercialPolicy(),
            new AllowAgencyOfferContentPolicy(),
            new AllowAgencyOfferChannelPolicy(),
            new AllowAgencyOfferPublicationPolicy());

        var decision = await evaluator.EvaluateAsync(SampleContext(), ct);
        Assert.True(decision.IsAllowed);
        Assert.Equal(AgencyOfferPolicyDecisionKind.Allow, decision.Kind);
        Assert.Equal("COMPOSITE_ALLOW", decision.Code);
        Assert.Null(typeof(AgencyOfferPolicyDecision).GetProperty("CommissionAmount"));
        Assert.Null(typeof(AgencyOfferPolicyContext).GetProperty("Settlement"));
    }

    [Fact]
    public async Task Composite_evaluator_returns_first_deny()
    {
        var ct = TestContext.Current.CancellationToken;
        var evaluator = new AgencyOfferPolicyEvaluator(
            new AllowAgencyOfferCommercialPolicy(),
            new DenyContentPolicy(),
            new AllowAgencyOfferChannelPolicy(),
            new AllowAgencyOfferPublicationPolicy());

        var decision = await evaluator.EvaluateAsync(SampleContext(), ct);
        Assert.False(decision.IsAllowed);
        Assert.Equal(AgencyOfferPolicyDecisionKind.Deny, decision.Kind);
        Assert.Equal("CONTENT_DENY_TEST", decision.Code);
        Assert.Equal(nameof(DenyContentPolicy), decision.PolicyName);
    }

    [Fact]
    public void Policy_denied_exception_carries_decision_without_money_fields()
    {
        var decision = AgencyOfferPolicyDecision.Deny(
            policyName: "Test",
            code: "DENY_CODE",
            reason: "Denied for test.");
        var ex = new AgencyOfferPolicyDeniedException(decision);
        Assert.Equal(decision, ex.Decision);
        Assert.Contains("DENY_CODE", ex.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("percent", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class DenyContentPolicy : IAgencyOfferContentPolicy
    {
        public Task<AgencyOfferPolicyDecision> EvaluateAsync(
            AgencyOfferPolicyContext context,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(AgencyOfferPolicyDecision.Deny(
                policyName: nameof(DenyContentPolicy),
                code: "CONTENT_DENY_TEST",
                reason: "Content policy denied for unit test."));
    }
}
