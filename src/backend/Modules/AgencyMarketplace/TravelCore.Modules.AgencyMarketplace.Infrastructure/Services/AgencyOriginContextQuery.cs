using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.AgencyMarketplace.Contracts;
using TravelCore.Modules.AgencyMarketplace.Domain;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Services;

/// <summary>
/// Trusted AgencyProfile / AgencyOffer identity read (TC-P19-T007).
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

        var offer = await _db.AgencyOffers
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == AgencyOfferId.From(agencyOfferId), cancellationToken);

        return offer is null ? null : Map(offer);
    }

    internal static AgencyOriginProfileFacts Map(AgencyProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return new AgencyOriginProfileFacts(profile.Id.Value, profile.Status.ToString());
    }

    internal static AgencyOriginOfferFacts Map(AgencyOffer offer)
    {
        ArgumentNullException.ThrowIfNull(offer);
        return new AgencyOriginOfferFacts(
            offer.Id.Value,
            offer.AgencyProfileId.Value,
            offer.TourProductId,
            offer.ReferencedTourDepartureId?.Value);
    }
}
