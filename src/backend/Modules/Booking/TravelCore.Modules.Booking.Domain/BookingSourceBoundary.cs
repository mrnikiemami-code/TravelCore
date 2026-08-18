namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// P19-R7 Direct/Agency source boundary. One Booking aggregate. No settlement, commission,
/// agency pricing, agency acceptance lifecycle, or agency capacity pool.
/// </summary>
public static class BookingSourceBoundary
{
    public const string BookingIsNotAgencyMarketplace = "Booking != AgencyMarketplace";
    public const string BookingSourceKindIsNotBookingStatus = "BookingSourceKind != BookingStatus";
    public const string AgencyOfferIsNotBooking = "AgencyOffer != Booking";
    public const string AgencyOfferIsNotQuote = "AgencyOffer != Quote";
    public const string AgencyOfferIsNotPrice = "AgencyOffer != Price";
    public const string AgencyContextIsNotPricingAuthority = "Agency context != Pricing Authority";
    public const string LeadIsNotBooking = "Lead != Booking";
    public const string VisaApplicationIsNotBooking = "VisaApplication != Booking";
    public const string BookingStatusIsNotAgencyOfferStatus = "BookingStatus != AgencyOfferStatus";
    public const string BookingStatusIsNotAgencyAcceptanceStatus = "BookingStatus != AgencyAcceptanceStatus";
    public const string AgencyAccessPolicy = "future object-level authorization; not globally visible";
    public const string AgencyOfferReferenceRequirement =
        "AgencyOfferReference is optional; AgencyProfileReference is required for Agency source";
    public const bool DirectAndAgencyUseSameAggregate = true;
    public const bool AgencyBookingAggregateImplemented = false;
    public const bool DirectBookingAggregateImplemented = false;
    public const bool AgencyPriceOverrideImplemented = false;
    public const bool CommissionImplemented = false;
    public const bool SettlementImplemented = false;
    public const bool AgencyAcceptanceLifecycleImplemented = false;
    public const bool AgencyCapacityPoolImplemented = false;
    public const bool AgencyPiiSharingImplemented = false;
    public const bool AgencyInboxImplemented = false;
    public const bool LeadConversionImplemented = false;
    public const bool SourceMutationImplemented = false;
}
