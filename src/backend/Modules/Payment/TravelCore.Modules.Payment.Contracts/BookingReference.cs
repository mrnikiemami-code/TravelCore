namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Opaque logical Booking identifier. Payment does not clone BookingStatus, passengers,
/// capacity, monetary amount, or TourDeparture (P20-R1).
/// </summary>
public readonly record struct BookingReference
{
    public Guid BookingId { get; }

    public BookingReference(Guid bookingId)
    {
        if (bookingId == Guid.Empty)
        {
            throw new ArgumentException(
                "BookingReference requires a non-empty Booking identifier.",
                nameof(bookingId));
        }

        BookingId = bookingId;
    }
}
