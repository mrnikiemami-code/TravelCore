namespace TravelCore.Modules.AgencyMarketplace.Contracts;

/// <summary>
/// Deterministic public AgencyOffer read by TourProduct (TC-P14-T007 / P14-R7).
/// Published + Listed + Active Agency + PublicListingEnabled. Not ranking / Booking / price.
/// </summary>
public static class RelatedAgencyOfferPublicEligibility
{
    public const int MaxItems = 6;

    public static bool IsOfferPubliclyEligible(
        string publicationStatus,
        string visibility,
        string offerStatus)
    {
        return string.Equals(publicationStatus, "Published", StringComparison.Ordinal)
            && string.Equals(visibility, "Listed", StringComparison.Ordinal)
            && !string.Equals(offerStatus, "Archived", StringComparison.Ordinal);
    }

    public static bool IsAgencyPubliclyEligible(string profileStatus, bool publicListingEnabled)
    {
        return publicListingEnabled
            && string.Equals(profileStatus, "Active", StringComparison.Ordinal);
    }
}

/// <summary>
/// Compact inquiry-oriented public read model. No money, commission, ranking, or IndexPolicy.
/// </summary>
public sealed record RelatedPublishedAgencyOffer(
    Guid AgencyOfferId,
    Guid AgencyProfileId,
    Guid TourProductId,
    string AgencyDisplayName,
    string? AgencyDescription,
    string? PublicEmail,
    string? PublicPhone,
    string? WebsiteUrl,
    string? TitleOverride,
    string? Highlight,
    bool RequiresManualConfirmation);

public interface IRelatedAgencyOfferPublicQuery
{
    Task<IReadOnlyList<RelatedPublishedAgencyOffer>> GetByTourProductAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default);
}
