using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.AgencyMarketplace.Contracts;
using TravelCore.Modules.AgencyMarketplace.Domain;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Services;

/// <summary>
/// AgencyOffer governance audit query (TC-P38-T013). Operational history only.
/// </summary>
internal sealed class AgencyOfferGovernanceAuditQuery : IAgencyOfferGovernanceAuditQuery
{
    private const int MaxTake = 200;
    private readonly AgencyMarketplaceDbContext _db;

    public AgencyOfferGovernanceAuditQuery(AgencyMarketplaceDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AgencyOfferGovernanceHistoryItem>> ListByOfferAsync(
        Guid offerId,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (offerId == Guid.Empty)
        {
            throw new ArgumentException("OfferId cannot be empty.", nameof(offerId));
        }

        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), "Take must be positive.");
        }

        if (take > MaxTake)
        {
            throw new ArgumentOutOfRangeException(nameof(take), $"Take cannot exceed {MaxTake}.");
        }

        var rows = await _db.AgencyOfferGovernanceEvents
            .AsNoTracking()
            .Where(x => x.OfferId == AgencyOfferId.From(offerId))
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

        return rows.Select(x => new AgencyOfferGovernanceHistoryItem(
            x.Id,
            x.OfferId.Value,
            x.AgencyProfileId.Value,
            x.Kind.ToString(),
            x.ActorKind,
            x.ActorAccountId,
            x.FromPublicationStatus,
            x.ToPublicationStatus,
            x.PolicyCode,
            x.PolicyName,
            x.Reason,
            x.OccurredAt.ToString())).ToList();
    }
}
