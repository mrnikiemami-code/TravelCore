namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Opaque logical Identity actor. Not an Identity Account entity (P19-R4).
/// </summary>
public readonly record struct BookingActorReference(Guid ActorId)
{
    public BookingActorReference()
        : this(Guid.Empty)
    {
    }
}
