namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Structured snapshot line kinds copied from accepted Pricing Quote contracts (Base/Fee/Tax).
/// Booking does not invent a tax/fee taxonomy.
/// </summary>
public enum BookingMonetaryComponentKind : short
{
    Base = 0,
    Fee = 1,
    Tax = 2
}
