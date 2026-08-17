namespace TravelCore.Modules.AgencyMarketplace.Domain;

/// <summary>
/// Non-price sales constraints on an AgencyOffer (TC-P13-T004 / P13-R4).
/// Not Pricing, not Discount, not Commission, not Booking.
/// </summary>
public sealed class AgencyOfferSalesRules
{
    private AgencyOfferSalesRules()
    {
    }

    public AgencyOfferSalesRules(bool requiresManualConfirmation, bool exclusiveListing)
    {
        RequiresManualConfirmation = requiresManualConfirmation;
        ExclusiveListing = exclusiveListing;
    }

    /// <summary>Sales must be confirmed by the agency. Does not create a Booking hold.</summary>
    public bool RequiresManualConfirmation { get; private set; }

    /// <summary>Commercial exclusivity intent for this listing. Does not lock TourProduct catalog ownership.</summary>
    public bool ExclusiveListing { get; private set; }

    public static AgencyOfferSalesRules Default() => new(requiresManualConfirmation: false, exclusiveListing: false);
}
