namespace TravelCore.Modules.HotelBooking.Contracts;

/// <summary>
/// Server-controlled source resolution. No ranking, failover, or user-selected adapter.
/// </summary>
public interface IHotelAvailabilitySourceResolver
{
    IHotelAvailabilitySource? Resolve(AvailabilitySourceKey sourceKey);

    IReadOnlyList<AvailabilitySourceKey> ListConfiguredKeys();
}
