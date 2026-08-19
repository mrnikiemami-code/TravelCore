namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Closed Payment target kinds for P22-R6. Not an open string TargetType platform.
/// Exact values: TourBooking, HotelBooking, FlightBooking.
/// </summary>
public enum PaymentTargetKind
{
    TourBooking = 1,
    HotelBooking = 2,
    FlightBooking = 3,
}
