namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Controlled Booking origin kinds (TC-P19-T007 / P19-R7). Not BookingStatus.
/// </summary>
public enum BookingSourceKind : short
{
    Direct = 0,
    Agency = 1
}
