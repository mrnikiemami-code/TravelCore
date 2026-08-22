using TravelCore.Modules.AgencyMarketplace.Contracts;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Policies;

/// <summary>
/// Composite AgencyOffer policy evaluator (TC-P38-T011).
/// Owned by AgencyMarketplace governance — not Pricing, not Booking.
/// Runs commercial → content → channel → publication; first Deny wins.
/// </summary>
internal sealed class AgencyOfferPolicyEvaluator : IAgencyOfferPolicyEvaluator
{
    private readonly IAgencyOfferCommercialPolicy _commercial;
    private readonly IAgencyOfferContentPolicy _content;
    private readonly IAgencyOfferChannelPolicy _channel;
    private readonly IAgencyOfferPublicationPolicy _publication;

    public AgencyOfferPolicyEvaluator(
        IAgencyOfferCommercialPolicy commercial,
        IAgencyOfferContentPolicy content,
        IAgencyOfferChannelPolicy channel,
        IAgencyOfferPublicationPolicy publication)
    {
        _commercial = commercial;
        _content = content;
        _channel = channel;
        _publication = publication;
    }

    public async Task<AgencyOfferPolicyDecision> EvaluateAsync(
        AgencyOfferPolicyContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var decision in await CollectAsync(context, cancellationToken))
        {
            if (!decision.IsAllowed)
            {
                return decision;
            }
        }

        return AgencyOfferPolicyDecision.Allow(
            policyName: nameof(AgencyOfferPolicyEvaluator),
            code: "COMPOSITE_ALLOW",
            reason: "All AgencyOffer policy hooks allowed.");
    }

    private async Task<IReadOnlyList<AgencyOfferPolicyDecision>> CollectAsync(
        AgencyOfferPolicyContext context,
        CancellationToken cancellationToken)
    {
        return
        [
            await _commercial.EvaluateAsync(context, cancellationToken),
            await _content.EvaluateAsync(context, cancellationToken),
            await _channel.EvaluateAsync(context, cancellationToken),
            await _publication.EvaluateAsync(context, cancellationToken)
        ];
    }
}

/// <summary>
/// Thrown when a governance mutation is denied by policy after authorization succeeded.
/// </summary>
public sealed class AgencyOfferPolicyDeniedException : InvalidOperationException
{
    public AgencyOfferPolicyDeniedException(AgencyOfferPolicyDecision decision)
        : base($"AgencyOffer policy denied ({decision.Code}): {decision.Reason}")
    {
        Decision = decision;
    }

    public AgencyOfferPolicyDecision Decision { get; }
}
