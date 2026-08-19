namespace TravelCore.Modules.DynamicPackage.Domain;

/// <summary>
/// Transient (non-persistent, non-transactional) DynamicPackage candidate built by composing:
/// exactly one FlightBooking component reference + exactly one HotelBooking component reference.
///
/// P23-R3: Search/composition authority boundary (no Search/Supplier truth).
/// </summary>
public sealed class TransientPackageCandidate
{
    private TransientPackageCandidate()
    {
        FlightComponent = default!;
        HotelComponent = default!;
    }

    private TransientPackageCandidate(
        FlightBookingId flightComponent,
        HotelBookingId hotelComponent)
    {
        if (flightComponent.Value == Guid.Empty)
        {
            throw new ArgumentException("Flight component reference cannot be empty.", nameof(flightComponent));
        }

        if (hotelComponent.Value == Guid.Empty)
        {
            throw new ArgumentException("Hotel component reference cannot be empty.", nameof(hotelComponent));
        }

        FlightComponent = flightComponent;
        HotelComponent = hotelComponent;
    }

    public FlightBookingId FlightComponent { get; private set; }

    public HotelBookingId HotelComponent { get; private set; }

    public static TransientPackageCandidate Create(
        FlightBookingId flightComponent,
        HotelBookingId hotelComponent)
    {
        return new TransientPackageCandidate(flightComponent, hotelComponent);
    }
}

