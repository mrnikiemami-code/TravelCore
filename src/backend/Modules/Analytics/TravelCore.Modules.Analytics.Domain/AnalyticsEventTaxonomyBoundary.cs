namespace TravelCore.Modules.Analytics.Domain;

/// <summary>
/// P27-R2 event taxonomy boundary marker. Taxonomy and ownership only — no dispatch persistence or provider execution in T005.
/// </summary>
public static class AnalyticsEventTaxonomyBoundary
{
    public const string EventTaxonomy =
        "SearchPerformed · SearchResultClicked · SearchNoResults · FilterApplied · TourViewed · HotelViewed · QuoteCreated · BookingStarted · BookingCompleted";

    public const string PublisherDoesNotCallVendorDirectly =
        "Domain modules must not call Mixpanel/GA/Amplitude/etc. directly";

    public const string TaxonomyOwner = "Analytics";
    public const string SearchPublisherOwner = "Search";
    public const string BookingPublisherOwner = "Booking";
    public const string PaymentPublisherOwner = "Payment";
    public const string TourPublisherOwner = "Tour";
    public const string HotelBookingPublisherOwner = "HotelBooking";

    public const bool AnalyticsOwnsEventTaxonomy = true;
    public const bool AnalyticsOwnsSearchRanking = false;
    public const bool AnalyticsOwnsBookingExecution = false;
    public const bool EventPersistenceImplemented = false;
    public const bool ProviderDispatchImplemented = false;
    public const bool MixpanelClientImplemented = false;
    public const bool GoogleAnalyticsClientImplemented = false;
    public const bool AmplitudeClientImplemented = false;
    public const bool SegmentRuntimeImplemented = false;
    public const bool PublicApiImplemented = false;
}
