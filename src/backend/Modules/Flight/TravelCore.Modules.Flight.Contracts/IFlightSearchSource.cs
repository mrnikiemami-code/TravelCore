namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Provider-neutral Flight search port. Production search source remains NONE.
/// Does not create FlightBooking or accepted fare truth.
/// </summary>
public interface IFlightSearchSource
{
    FlightSourceKey Key { get; }

    IReadOnlySet<FlightSourceCapability> Capabilities { get; }

    Task<FlightSearchResult> SearchAsync(
        FlightSearchRequest request,
        CancellationToken cancellationToken = default);
}
