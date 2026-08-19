using TravelCore.Modules.Analytics.Contracts;

namespace TravelCore.Modules.Analytics.Domain;

/// <summary>
/// Logical product event intent reference for Analytics-owned taxonomy. No persistence in T005.
/// </summary>
public readonly record struct AnalyticsEventReference(AnalyticsProductEventKind Kind)
{
    public static AnalyticsEventReference SearchPerformed =>
        new(AnalyticsProductEventKind.SearchPerformed);

    public static AnalyticsEventReference SearchResultClicked =>
        new(AnalyticsProductEventKind.SearchResultClicked);

    public static AnalyticsEventReference SearchNoResults =>
        new(AnalyticsProductEventKind.SearchNoResults);

    public static AnalyticsEventReference FilterApplied =>
        new(AnalyticsProductEventKind.FilterApplied);

    public static AnalyticsEventReference TourViewed =>
        new(AnalyticsProductEventKind.TourViewed);

    public static AnalyticsEventReference HotelViewed =>
        new(AnalyticsProductEventKind.HotelViewed);

    public static AnalyticsEventReference QuoteCreated =>
        new(AnalyticsProductEventKind.QuoteCreated);

    public static AnalyticsEventReference BookingStarted =>
        new(AnalyticsProductEventKind.BookingStarted);

    public static AnalyticsEventReference BookingCompleted =>
        new(AnalyticsProductEventKind.BookingCompleted);
}
