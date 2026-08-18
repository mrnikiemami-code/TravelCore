namespace TravelCore.Modules.HotelBooking.Contracts;

/// <summary>
/// Provider-neutral final hotel reservation port. Named production source remains NONE.
/// </summary>
public interface IHotelReservationSource
{
    ReservationSourceKey Key { get; }

    bool RequiresActiveHold { get; }

    bool NotFoundProvesNoReservation { get; }

    Task<HotelReservationSourceResult> CreateReservationAsync(
        HotelReservationRequest request,
        CancellationToken cancellationToken = default);

    Task<HotelReservationQueryResult> QueryReservationStatusAsync(
        string sourceReservationReference,
        CancellationToken cancellationToken = default);
}
