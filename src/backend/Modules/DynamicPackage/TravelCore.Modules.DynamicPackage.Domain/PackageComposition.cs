namespace TravelCore.Modules.DynamicPackage.Domain;

/// <summary>
/// Dynamic Package composition boundary: exactly one FlightBooking reference and exactly one HotelBooking reference.
/// No orchestration, payment, reservation, saga, lifecycle, or public journey state.
/// </summary>
public sealed class PackageComposition
{
    private PackageComposition()
    {
        FlightBookingId = default!;
        HotelBookingId = default!;
        Id = default!;
    }

    private PackageComposition(
        PackageCompositionId id,
        FlightBookingId flightBookingId,
        HotelBookingId hotelBookingId)
    {
        // Invariant: references required (no default/empty ids).
        if (flightBookingId.Value == Guid.Empty)
        {
            throw new ArgumentException("FlightBookingId cannot be empty.", nameof(flightBookingId));
        }

        if (hotelBookingId.Value == Guid.Empty)
        {
            throw new ArgumentException("HotelBookingId cannot be empty.", nameof(hotelBookingId));
        }

        Id = id;
        FlightBookingId = flightBookingId;
        HotelBookingId = hotelBookingId;
    }

    public PackageCompositionId Id { get; private set; }

    public FlightBookingId FlightBookingId { get; private set; }

    public HotelBookingId HotelBookingId { get; private set; }

    public static PackageComposition Create(
        FlightBookingId flightBookingId,
        HotelBookingId hotelBookingId)
    {
        return new PackageComposition(
            PackageCompositionId.New(),
            flightBookingId,
            hotelBookingId);
    }
}

