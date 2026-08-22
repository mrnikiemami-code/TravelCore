using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.AgencyMarketplace.Contracts;
using TravelCore.Modules.AgencyMarketplace.Domain;
using TravelCore.Modules.AgencyMarketplace.Infrastructure.Policies;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Services;

/// <summary>
/// Admin AgencyOffer governance (TC-P38-T010).
/// Agency creates/submits; Admin approves/governs; Public consumes Published only.
/// Agency cannot moderate its own offer. Financial engines are out of scope.
/// </summary>
internal sealed class AgencyOfferGovernanceService : IAgencyOfferGovernanceService
{
    private const int MaxTake = 200;

    private readonly AgencyMarketplaceDbContext _db;
    private readonly IAgencyOfferPolicyEvaluator _policyEvaluator;

    public AgencyOfferGovernanceService(
        AgencyMarketplaceDbContext db,
        IAgencyOfferPolicyEvaluator policyEvaluator)
    {
        _db = db;
        _policyEvaluator = policyEvaluator;
    }

    public async Task<IReadOnlyList<AgencyOfferModerationQueueItem>> ListPendingOffersAsync(
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

        var rows = await _db.AgencyOffers
            .AsNoTracking()
            .Where(x => x.PublicationStatus == AgencyOfferPublicationStatus.Submitted)
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

        return rows.Select(MapItem).ToList();
    }

    public Task<AgencyOfferModerationQueueItem> ApproveOfferAsync(
        Guid offerId,
        Guid? actingAgencyProfileId,
        CancellationToken cancellationToken = default) =>
        MutateAsync(offerId, actingAgencyProfileId, offer => offer.Approve(), cancellationToken);

    public Task<AgencyOfferModerationQueueItem> RejectOfferAsync(
        Guid offerId,
        Guid? actingAgencyProfileId,
        CancellationToken cancellationToken = default) =>
        MutateAsync(offerId, actingAgencyProfileId, offer => offer.Reject(), cancellationToken);

    public Task<AgencyOfferModerationQueueItem> SuspendOfferAsync(
        Guid offerId,
        Guid? actingAgencyProfileId,
        CancellationToken cancellationToken = default) =>
        MutateAsync(offerId, actingAgencyProfileId, offer => offer.Suspend(), cancellationToken);

    private async Task<AgencyOfferModerationQueueItem> MutateAsync(
        Guid offerId,
        Guid? actingAgencyProfileId,
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

        var policyContext = ToPolicyContext(offer);
        var decision = await _policyEvaluator.EvaluateAsync(policyContext, cancellationToken);
        if (!decision.IsAllowed)
        {
            throw new AgencyOfferPolicyDeniedException(decision);
        }

        mutate(offer);
        await _db.SaveChangesAsync(cancellationToken);
        return MapItem(offer);
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

    private static AgencyOfferModerationQueueItem MapItem(AgencyOffer offer) =>
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
            offer.UpdatedAt.ToString());
}
