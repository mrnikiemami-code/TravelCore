namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Opaque logical HotelBooking identifier. Not a Payment authorization credential (P21-R6).
/// </summary>
public readonly record struct HotelBookingPaymentReference
{
    public Guid HotelBookingId { get; }

    public HotelBookingPaymentReference(Guid hotelBookingId)
    {
        if (hotelBookingId == Guid.Empty)
        {
            throw new ArgumentException(
                "HotelBookingPaymentReference requires a non-empty HotelBooking identifier.",
                nameof(hotelBookingId));
        }

        HotelBookingId = hotelBookingId;
    }
}
