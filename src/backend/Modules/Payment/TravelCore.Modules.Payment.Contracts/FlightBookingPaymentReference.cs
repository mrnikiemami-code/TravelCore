namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Opaque logical FlightBooking identifier. Not a Payment authorization credential (P22-R6).
/// </summary>
public readonly record struct FlightBookingPaymentReference
{
    public Guid FlightBookingId { get; }

    public FlightBookingPaymentReference(Guid flightBookingId)
    {
        if (flightBookingId == Guid.Empty)
        {
            throw new ArgumentException(
                "FlightBookingPaymentReference requires a non-empty FlightBooking identifier.",
                nameof(flightBookingId));
        }

        FlightBookingId = flightBookingId;
    }
}
