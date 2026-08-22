using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.AgencyMarketplace.Contracts;
using TravelCore.Modules.AgencyMarketplace.Domain;
using TravelCore.Modules.AgencyMarketplace.Infrastructure.Policies;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Services;

/// <summary>
/// Admin AgencyOffer governance (TC-P38-T010 / T013 / T014).
/// Agency creates/submits; Admin approves/governs; Public consumes Published only.
/// Records operational governance history — not a financial ledger.
/// </summary>
internal sealed class AgencyOfferGovernanceService : IAgencyOfferGovernanceService
{
    private const int MaxTake = 200;

    private static readonly HashSet<AgencyOfferPublicationStatus> OpsFilterStatuses =
    [
        AgencyOfferPublicationStatus.Submitted,
        AgencyOfferPublicationStatus.Approved,
        AgencyOfferPublicationStatus.Rejected,
        AgencyOfferPublicationStatus.Suspended,
        AgencyOfferPublicationStatus.Retired
    ];

    private readonly AgencyMarketplaceDbContext _db;
    private readonly IAgencyOfferPolicyEvaluator _policyEvaluator;

    public AgencyOfferGovernanceService(
        AgencyMarketplaceDbContext db,
        IAgencyOfferPolicyEvaluator policyEvaluator)
    {
        _db = db;
        _policyEvaluator = policyEvaluator;
    }

    public Task<IReadOnlyList<AgencyOfferModerationQueueItem>> ListPendingOffersAsync(
        int take,
        CancellationToken cancellationToken = default) =>
        ListOffersAsync("Submitted", take, cancellationToken);

    public async Task<IReadOnlyList<AgencyOfferModerationQueueItem>> ListOffersAsync(
        string? publicationStatus,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), "Take must be positive.");
        }

        if (take > MaxTake)
        {
            throw new ArgumentOutOfRangeException(nameof(take), $"Take cannot exceed {MaxTake}.");
        }

        var status = ParseOpsPublicationStatus(publicationStatus);

        var rows = await _db.AgencyOffers
            .AsNoTracking()
            .Where(x => x.PublicationStatus == status)
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

        return await MapItemsWithGovernanceVisibilityAsync(rows, cancellationToken);
    }

    internal static AgencyOfferPublicationStatus ParseOpsPublicationStatus(string? publicationStatus)
    {
        var raw = string.IsNullOrWhiteSpace(publicationStatus)
            ? "Submitted"
            : publicationStatus.Trim();

        if (raw.Equals("pending", StringComparison.OrdinalIgnoreCase)
            || raw.Equals("pendingreview", StringComparison.OrdinalIgnoreCase)
            || raw.Equals("pending-review", StringComparison.OrdinalIgnoreCase))
        {
            return AgencyOfferPublicationStatus.Submitted;
        }

        if (!Enum.TryParse<AgencyOfferPublicationStatus>(raw, ignoreCase: true, out var status)
            || !OpsFilterStatuses.Contains(status))
        {
            throw new ArgumentException(
                "publicationStatus must be one of: Submitted (pending), Approved, Rejected, Suspended, Retired.",
                nameof(publicationStatus));
        }

        return status;
    }

    public Task<AgencyOfferModerationQueueItem> ApproveOfferAsync(
        Guid offerId,
        Guid? actingAgencyProfileId,
        Guid? actorAccountId = null,
        CancellationToken cancellationToken = default) =>
        MutateAsync(
            offerId,
            actingAgencyProfileId,
            actorAccountId,
            AgencyOfferGovernanceEventKind.Approved,
            offer => offer.Approve(),
            cancellationToken);

    public Task<AgencyOfferModerationQueueItem> RejectOfferAsync(
        Guid offerId,
        Guid? actingAgencyProfileId,
        Guid? actorAccountId = null,
        CancellationToken cancellationToken = default) =>
        MutateAsync(
            offerId,
            actingAgencyProfileId,
            actorAccountId,
            AgencyOfferGovernanceEventKind.Rejected,
            offer => offer.Reject(),
            cancellationToken);

    public Task<AgencyOfferModerationQueueItem> SuspendOfferAsync(
        Guid offerId,
        Guid? actingAgencyProfileId,
        Guid? actorAccountId = null,
        CancellationToken cancellationToken = default) =>
        MutateAsync(
            offerId,
            actingAgencyProfileId,
            actorAccountId,
            AgencyOfferGovernanceEventKind.Suspended,
            offer => offer.Suspend(),
            cancellationToken);

    public async Task<AgencyOfferPolicyEvaluationReport> EvaluateOfferPoliciesAsync(
        Guid offerId,
        CancellationToken cancellationToken = default)
    {
        if (offerId == Guid.Empty)
        {
            throw new ArgumentException("OfferId cannot be empty.", nameof(offerId));
        }

        var offer = await _db.AgencyOffers.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == AgencyOfferId.From(offerId), cancellationToken)
            ?? throw new KeyNotFoundException("AgencyOffer was not found.");

        return await _policyEvaluator.EvaluateDetailedAsync(ToPolicyContext(offer), cancellationToken);
    }

    private async Task<AgencyOfferModerationQueueItem> MutateAsync(
        Guid offerId,
        Guid? actingAgencyProfileId,
        Guid? actorAccountId,
        AgencyOfferGovernanceEventKind eventKind,
        Action<AgencyOffer> mutate,
        CancellationToken cancellationToken)
    {
        if (offerId == Guid.Empty)
        {
            throw new ArgumentException("OfferId cannot be empty.", nameof(offerId));
        }

        var offer = await _db.AgencyOffers
            .SingleOrDefaultAsync(x => x.Id == AgencyOfferId.From(offerId), cancellationToken)
            ?? throw new KeyNotFoundException("AgencyOffer was not found.");

        EnsureNotSelfModeration(offer, actingAgencyProfileId);

        var fromStatus = offer.PublicationStatus.ToString();
        var policyContext = ToPolicyContext(offer);
        var decision = await _policyEvaluator.EvaluateAsync(policyContext, cancellationToken);
        if (!decision.IsAllowed)
        {
            _db.AgencyOfferGovernanceEvents.Add(AgencyOfferGovernanceEvent.Create(
                offer.Id,
                offer.AgencyProfileId,
                AgencyOfferGovernanceEventKind.PolicyDenied,
                actorKind: "Admin",
                actorAccountId: actorAccountId,
                fromPublicationStatus: fromStatus,
                toPublicationStatus: fromStatus,
                policyCode: decision.Code,
                policyName: decision.PolicyName,
                reason: decision.Reason));
            await _db.SaveChangesAsync(cancellationToken);
            throw new AgencyOfferPolicyDeniedException(decision);
        }

        mutate(offer);
        _db.AgencyOfferGovernanceEvents.Add(AgencyOfferGovernanceEvent.Create(
            offer.Id,
            offer.AgencyProfileId,
            eventKind,
            actorKind: "Admin",
            actorAccountId: actorAccountId,
            fromPublicationStatus: fromStatus,
            toPublicationStatus: offer.PublicationStatus.ToString()));
        await _db.SaveChangesAsync(cancellationToken);
        var enriched = await MapItemsWithGovernanceVisibilityAsync([offer], cancellationToken);
        return enriched[0];
    }

    /// <summary>
    /// Agency operators must not approve/reject/suspend their own offers even if they somehow hold Moderate.
    /// Pure Admins (no acting AgencyProfile) are allowed.
    /// </summary>
    internal static void EnsureNotSelfModeration(AgencyOffer offer, Guid? actingAgencyProfileId)
    {
        if (actingAgencyProfileId is null)
        {
            return;
        }

        if (offer.AgencyProfileId == AgencyProfileId.From(actingAgencyProfileId.Value))
        {
            throw new UnauthorizedAccessException(
                "Agency cannot moderate its own AgencyOffer.");
        }
    }

    private static AgencyOfferPolicyContext ToPolicyContext(AgencyOffer offer) =>
        new(
            offer.Id.Value,
            offer.AgencyProfileId.Value,
            offer.TourProductId,
            offer.SalesChannel.ToString(),
            offer.PublicationStatus.ToString(),
            offer.Visibility.ToString(),
            offer.Status.ToString());

    private async Task<IReadOnlyList<AgencyOfferModerationQueueItem>> MapItemsWithGovernanceVisibilityAsync(
        IReadOnlyList<AgencyOffer> offers,
        CancellationToken cancellationToken)
    {
        if (offers.Count == 0)
        {
            return [];
        }

        var offerIds = offers.Select(x => x.Id).ToList();
        var events = await _db.AgencyOfferGovernanceEvents
            .AsNoTracking()
            .Where(x => offerIds.Contains(x.OfferId))
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

        var latestByOffer = events
            .GroupBy(x => x.OfferId)
            .ToDictionary(g => g.Key, g => g.First());

        return offers.Select(offer =>
        {
            latestByOffer.TryGetValue(offer.Id, out var latest);
            return MapItem(
                offer,
                lastDecisionKind: latest?.Kind.ToString(),
                lastDecisionAt: latest?.OccurredAt.ToString(),
                hasGovernanceHistory: latest is not null);
        }).ToList();
    }

    private static AgencyOfferModerationQueueItem MapItem(
        AgencyOffer offer,
        string? lastDecisionKind = null,
        string? lastDecisionAt = null,
        bool hasGovernanceHistory = false) =>
        new(
            offer.Id.Value,
            offer.AgencyProfileId.Value,
            offer.TourProductId,
            offer.Display.TitleOverride,
            offer.Display.Highlight,
            offer.SalesChannel.ToString(),
            offer.Status.ToString(),
            offer.Visibility.ToString(),
            offer.PublicationStatus.ToString(),
            offer.CreatedAt.ToString(),
            offer.UpdatedAt.ToString(),
            lastDecisionKind,
            lastDecisionAt,
            hasGovernanceHistory);
}
