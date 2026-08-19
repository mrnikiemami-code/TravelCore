namespace TravelCore.Modules.Analytics.Contracts;

/// <summary>
/// P27-R1 publisher vs dispatch-owner posture. Business modules emit semantic product events only.
/// </summary>
public static class AnalyticsPublisherBoundary
{
    public const string SemanticEventPublisherPosture =
        "Business modules publish semantic product events; Analytics owns taxonomy and dispatch orchestration";

    public const string SearchPublisherOwner = "Search";
    public const string BookingPublisherOwner = "Booking";
    public const string PaymentPublisherOwner = "Payment";
    public const string TourPublisherOwner = "Tour";
    public const string HotelBookingPublisherOwner = "HotelBooking";
    public const string DispatchOwner = "Analytics";
}
