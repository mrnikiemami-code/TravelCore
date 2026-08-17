using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.AgencyMarketplace.Contracts;
using TravelCore.Modules.AgencyMarketplace.Domain;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Services;

/// <summary>
/// Deterministic Published AgencyOffer read by TourProduct (TC-P14-T007). No score / ranking.
/// </summary>
public sealed class RelatedAgencyOfferPublicQuery : IRelatedAgencyOfferPublicQuery
{
    private readonly AgencyMarketplaceDbContext _db;

    public RelatedAgencyOfferPublicQuery(AgencyMarketplaceDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RelatedPublishedAgencyOffer>> GetByTourProductAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default)
    {
        if (tourProductId == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        var rows = await (
                from offer in _db.AgencyOffers.AsNoTracking()
                join profile in _db.AgencyProfiles.AsNoTracking()
                    on offer.AgencyProfileId equals profile.Id
                where offer.TourProductId == tourProductId
                select new { offer, profile })
            .ToListAsync(cancellationToken);

        return rows
            .Where(x =>
                RelatedAgencyOfferPublicEligibility.IsOfferPubliclyEligible(
                    x.offer.PublicationStatus.ToString(),
                    x.offer.Visibility.ToString(),
                    x.offer.Status.ToString())
                && RelatedAgencyOfferPublicEligibility.IsAgencyPubliclyEligible(
                    x.profile.Status.ToString(),
                    x.profile.Commercial.PublicListingEnabled))
            .OrderBy(x => x.profile.Display.DisplayName, StringComparer.Ordinal)
            .ThenBy(x => x.offer.Id.Value)
            .Take(RelatedAgencyOfferPublicEligibility.MaxItems)
            .Select(x => new RelatedPublishedAgencyOffer(
                x.offer.Id.Value,
                x.profile.Id.Value,
                x.offer.TourProductId,
                x.profile.Display.DisplayName,
                x.profile.Display.Description,
                x.profile.Contact.PublicEmail,
                x.profile.Contact.PublicPhone,
                x.profile.Contact.WebsiteUrl,
                x.offer.Display.TitleOverride,
                x.offer.Display.Highlight,
                x.offer.CommercialTerms.SalesRules.RequiresManualConfirmation))
            .ToList();
    }
}
