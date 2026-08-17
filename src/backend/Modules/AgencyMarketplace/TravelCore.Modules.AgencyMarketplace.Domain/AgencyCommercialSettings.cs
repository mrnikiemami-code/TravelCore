namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Commercial marketplace posture only. Not Pricing, not commission, not Booking.
/// </summary>
public sealed class AgencyCommercialSettings
{
    private AgencyCommercialSettings()
    {
    }

    public AgencyCommercialSettings(bool publicListingEnabled)
    {
        PublicListingEnabled = publicListingEnabled;
    }

    /// <summary>
    /// Readiness flag for later public listing. Does not imply Offer/publish workflow (P13-R7).
    /// </summary>
    public bool PublicListingEnabled { get; private set; }

    public static AgencyCommercialSettings Default() => new(publicListingEnabled: false);
}
