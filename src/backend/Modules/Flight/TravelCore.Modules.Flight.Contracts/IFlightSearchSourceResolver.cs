namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Server-controlled Flight search source resolution. No ranking, failover, or client-selected adapter.
/// </summary>
public interface IFlightSearchSourceResolver
{
    IFlightSearchSource? Resolve(FlightSourceKey sourceKey);

    IReadOnlyList<FlightSourceKey> ListConfiguredKeys();
}
