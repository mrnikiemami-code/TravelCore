namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Opaque logical Party id. Not a Party person master clone (P19-R4).
/// </summary>
public readonly record struct BookingPartyReference(Guid PartyId)
{
    public BookingPartyReference()
        : this(Guid.Empty)
    {
    }
}
