namespace TravelCore.Modules.HotelBooking.Contracts;

/// <summary>
/// Server-controlled reservation source resolution. No ranking, failover, or user-selected adapter.
/// </summary>
public interface IHotelReservationSourceResolver
{
    IHotelReservationSource? Resolve(ReservationSourceKey sourceKey);

    IReadOnlyList<ReservationSourceKey> ListConfiguredKeys();
}
