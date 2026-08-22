namespace TravelCore.Modules.AgencyMarketplace.Contracts;

/// <summary>
/// Admin AgencyOffer governance contracts (TC-P38-T010).
/// Operational approval/control only — no Commission, Settlement, or Payout.
/// </summary>
public sealed record AgencyOfferModerationQueueItem(
    Guid OfferId,
    Guid AgencyProfileId,
    Guid TourProductId,
    string? TitleOverride,
    string? Highlight,
    string SalesChannel,
    string Status,
    string Visibility,
    string PublicationStatus,
    string CreatedAt,
    string UpdatedAt);

/// <summary>
/// Context passed to future policy extension points before governance mutations.
/// Intentionally excludes money fields (P38-T010/T011).
/// </summary>
public sealed record AgencyOfferPolicyContext(
    Guid OfferId,
    Guid AgencyProfileId,
    Guid TourProductId,
    string SalesChannel,
    string PublicationStatus,
    string Visibility,
    string OfferStatus);

/// <summary>
/// Commercial-rules hook. Default Allow — no financial math (P38-T011).
/// </summary>
public interface IAgencyOfferCommercialPolicy
{
    Task<AgencyOfferPolicyDecision> EvaluateAsync(
        AgencyOfferPolicyContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Content-policy hook (copy / media / honesty). Default Allow (P38-T011).
/// </summary>
public interface IAgencyOfferContentPolicy
{
    Task<AgencyOfferPolicyDecision> EvaluateAsync(
        AgencyOfferPolicyContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Channel-policy hook (Public / AgencyPortal / Private). Default Allow (P38-T011).
/// </summary>
public interface IAgencyOfferChannelPolicy
{
    Task<AgencyOfferPolicyDecision> EvaluateAsync(
        AgencyOfferPolicyContext context,
        CancellationToken cancellationToken = default);
}

public interface IAgencyOfferGovernanceService
{
    Task<IReadOnlyList<AgencyOfferModerationQueueItem>> ListPendingOffersAsync(
        int take,
        CancellationToken cancellationToken = default);

    Task<AgencyOfferModerationQueueItem> ApproveOfferAsync(
        Guid offerId,
        Guid? actingAgencyProfileId,
        CancellationToken cancellationToken = default);

    Task<AgencyOfferModerationQueueItem> RejectOfferAsync(
        Guid offerId,
        Guid? actingAgencyProfileId,
        CancellationToken cancellationToken = default);

    Task<AgencyOfferModerationQueueItem> SuspendOfferAsync(
        Guid offerId,
        Guid? actingAgencyProfileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Operational policy visibility for an offer (P38-T012). Does not mutate.
    /// </summary>
    Task<AgencyOfferPolicyEvaluationReport> EvaluateOfferPoliciesAsync(
        Guid offerId,
        CancellationToken cancellationToken = default);
}
