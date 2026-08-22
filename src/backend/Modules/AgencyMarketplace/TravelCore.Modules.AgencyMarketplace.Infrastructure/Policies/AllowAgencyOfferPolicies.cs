using TravelCore.Modules.AgencyMarketplace.Contracts;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Policies;

/// <summary>
/// Default commercial policy stub (P38-T010/T011). Always Allows — extension point only.
/// </summary>
internal sealed class AllowAgencyOfferCommercialPolicy : IAgencyOfferCommercialPolicy
{
    public Task<AgencyOfferPolicyDecision> EvaluateAsync(
        AgencyOfferPolicyContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return Task.FromResult(AgencyOfferPolicyDecision.Allow(
            policyName: nameof(AllowAgencyOfferCommercialPolicy),
            code: "COMMERCIAL_ALLOW_DEFAULT"));
    }
}

/// <summary>
/// Default content policy stub (P38-T010/T011). Always Allows — extension point only.
/// </summary>
internal sealed class AllowAgencyOfferContentPolicy : IAgencyOfferContentPolicy
{
    public Task<AgencyOfferPolicyDecision> EvaluateAsync(
        AgencyOfferPolicyContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return Task.FromResult(AgencyOfferPolicyDecision.Allow(
            policyName: nameof(AllowAgencyOfferContentPolicy),
            code: "CONTENT_ALLOW_DEFAULT"));
    }
}

/// <summary>
/// Default channel policy stub (P38-T010/T011). Always Allows — extension point only.
/// </summary>
internal sealed class AllowAgencyOfferChannelPolicy : IAgencyOfferChannelPolicy
{
    public Task<AgencyOfferPolicyDecision> EvaluateAsync(
        AgencyOfferPolicyContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return Task.FromResult(AgencyOfferPolicyDecision.Allow(
            policyName: nameof(AllowAgencyOfferChannelPolicy),
            code: "CHANNEL_ALLOW_DEFAULT"));
    }
}

/// <summary>
/// Default publication policy stub (P38-T011). Always Allows — extension point only.
/// </summary>
internal sealed class AllowAgencyOfferPublicationPolicy : IAgencyOfferPublicationPolicy
{
    public Task<AgencyOfferPolicyDecision> EvaluateAsync(
        AgencyOfferPolicyContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return Task.FromResult(AgencyOfferPolicyDecision.Allow(
            policyName: nameof(AllowAgencyOfferPublicationPolicy),
            code: "PUBLICATION_ALLOW_DEFAULT"));
    }
}
