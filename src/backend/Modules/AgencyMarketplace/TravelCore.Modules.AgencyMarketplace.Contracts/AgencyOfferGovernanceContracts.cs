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
/// Intentionally excludes money / commission / settlement fields.
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
/// Future commercial-rules hook. Current implementation must Allow — no commission.
/// </summary>
public interface IAgencyOfferCommercialPolicy
{
    Task EnsureAllowsAsync(AgencyOfferPolicyContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Future content-policy hook (copy / media / honesty). Current implementation must Allow.
/// </summary>
public interface IAgencyOfferContentPolicy
{
    Task EnsureAllowsAsync(AgencyOfferPolicyContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Future channel-policy hook (Public / AgencyPortal / Private). Current implementation must Allow.
/// </summary>
public interface IAgencyOfferChannelPolicy
{
    Task EnsureAllowsAsync(AgencyOfferPolicyContext context, CancellationToken cancellationToken = default);
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
}
