namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Server-controlled reservation source resolution. No ranking, failover, or client-selected adapter.
/// </summary>
public interface IFlightReservationSourceResolver
{
    IFlightReservationSource? Resolve(FlightSourceKey sourceKey);

    IReadOnlyList<FlightSourceKey> ListConfiguredKeys();
}
