namespace TravelCore.Modules.HotelBooking.Contracts;

/// <summary>
/// Provider-neutral authoritative hotel availability port. Named production source remains NONE.
/// </summary>
public interface IHotelAvailabilitySource
{
    AvailabilitySourceKey Key { get; }

    Task<HotelAvailabilityHoldSourceResult> CheckAvailabilityAsync(
        HotelAvailabilityRequest request,
        CancellationToken cancellationToken = default);

    Task<HotelAvailabilityHoldSourceResult> CreateHoldAsync(
        HotelAvailabilityRequest request,
        CancellationToken cancellationToken = default);

    Task<HotelAvailabilityHoldQueryResult> QueryHoldStatusAsync(
        string sourceHoldReference,
        CancellationToken cancellationToken = default);

    Task ReleaseHoldAsync(
        string sourceHoldReference,
        CancellationToken cancellationToken = default);
}
