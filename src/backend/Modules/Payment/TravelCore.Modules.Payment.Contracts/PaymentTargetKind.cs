namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Closed Payment target kinds for P21-R6. Not an open string TargetType platform.
/// Exact baseline values: TourBooking and HotelBooking only.
/// </summary>
public enum PaymentTargetKind
{
    TourBooking = 1,
    HotelBooking = 2,
}
