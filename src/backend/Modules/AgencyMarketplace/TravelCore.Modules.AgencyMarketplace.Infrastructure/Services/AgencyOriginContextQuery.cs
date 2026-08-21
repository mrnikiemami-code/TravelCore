using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.AgencyMarketplace.Contracts;
using TravelCore.Modules.AgencyMarketplace.Domain;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Services;

/// <summary>
/// Trusted AgencyProfile / AgencyOffer identity read (TC-P19-T007 / P38-T005).
/// Does not mutate marketplace aggregates and does not expose peer Booking types.
/// </summary>
public sealed class AgencyOriginContextQuery : IAgencyOriginContextQuery
{
    private readonly AgencyMarketplaceDbContext _db;

    public AgencyOriginContextQuery(AgencyMarketplaceDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    public async Task<AgencyOriginProfileFacts?> GetProfileAsync(
        Guid agencyProfileId,
        CancellationToken cancellationToken = default)
    {
        if (agencyProfileId == Guid.Empty)
        {
            throw new ArgumentException("AgencyProfileId cannot be empty.", nameof(agencyProfileId));
        }

        var profile = await _db.AgencyProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == AgencyProfileId.From(agencyProfileId), cancellationToken);

        return profile is null ? null : Map(profile);
    }

    public async Task<AgencyOriginOfferFacts?> GetOfferAsync(
        Guid agencyOfferId,
        CancellationToken cancellationToken = default)
    {
        if (agencyOfferId == Guid.Empty)
        {
            throw new ArgumentException("AgencyOfferId cannot be empty.", nameof(agencyOfferId));
        }

        var row = await (
                from offer in _db.AgencyOffers.AsNoTracking()
                join profile in _db.AgencyProfiles.AsNoTracking()
                    on offer.AgencyProfileId equals profile.Id
                where offer.Id == AgencyOfferId.From(agencyOfferId)
                select new { offer, profile })
            .SingleOrDefaultAsync(cancellationToken);

        return row is null ? null : Map(row.offer, row.profile);
    }

    internal static AgencyOriginProfileFacts Map(AgencyProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return new AgencyOriginProfileFacts(profile.Id.Value, profile.Status.ToString());
    }

    internal static AgencyOriginOfferFacts Map(AgencyOffer offer, AgencyProfile profile)
    {
        ArgumentNullException.ThrowIfNull(offer);
        ArgumentNullException.ThrowIfNull(profile);
        return new AgencyOriginOfferFacts(
            offer.Id.Value,
            offer.AgencyProfileId.Value,
            offer.TourProductId,
            offer.ReferencedTourDepartureId?.Value,
            offer.DepartureScopeMode.ToString(),
            offer.DepartureScopeIds.ToArray(),
            offer.PublicationStatus.ToString(),
            offer.Visibility.ToString(),
            offer.Status.ToString(),
            offer.SalesChannel.ToString(),
            profile.Status.ToString(),
            profile.Commercial.PublicListingEnabled);
    }
}
