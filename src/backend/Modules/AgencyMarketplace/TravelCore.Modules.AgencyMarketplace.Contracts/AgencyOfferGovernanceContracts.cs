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
    string UpdatedAt,
    /// <summary>Latest operational governance event kind (not financial).</summary>
    string? LastDecisionKind = null,
    string? LastDecisionAt = null,
    bool HasGovernanceHistory = false);

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

    /// <summary>
    /// Operational governance search by publication status (TC-P38-T014).
    /// Allowed: Submitted (pending), Approved, Rejected, Suspended, Retired.
    /// </summary>
    Task<IReadOnlyList<AgencyOfferModerationQueueItem>> ListOffersAsync(
        string? publicationStatus,
        int take,
        CancellationToken cancellationToken = default);

    Task<AgencyOfferModerationQueueItem> ApproveOfferAsync(
        Guid offerId,
        Guid? actingAgencyProfileId,
        Guid? actorAccountId = null,
        CancellationToken cancellationToken = default);

    Task<AgencyOfferModerationQueueItem> RejectOfferAsync(
        Guid offerId,
        Guid? actingAgencyProfileId,
        Guid? actorAccountId = null,
        CancellationToken cancellationToken = default);

    Task<AgencyOfferModerationQueueItem> SuspendOfferAsync(
        Guid offerId,
        Guid? actingAgencyProfileId,
        Guid? actorAccountId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Operational policy visibility for an offer (P38-T012). Does not mutate.
    /// </summary>
    Task<AgencyOfferPolicyEvaluationReport> EvaluateOfferPoliciesAsync(
        Guid offerId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Operational governance history (TC-P38-T013). Not a financial ledger.
/// </summary>
public sealed record AgencyOfferGovernanceHistoryItem(
    Guid EventId,
    Guid OfferId,
    Guid AgencyProfileId,
    string Kind,
    string ActorKind,
    Guid? ActorAccountId,
    string? FromPublicationStatus,
    string? ToPublicationStatus,
    string? PolicyCode,
    string? PolicyName,
    string? Reason,
    string OccurredAt);

public interface IAgencyOfferGovernanceAuditQuery
{
    Task<IReadOnlyList<AgencyOfferGovernanceHistoryItem>> ListByOfferAsync(
        Guid offerId,
        int take,
        CancellationToken cancellationToken = default);
}
