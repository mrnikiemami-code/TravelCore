namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Server-controlled Flight availability source resolution. No ranking, failover, or client-selected adapter.
/// </summary>
public interface IFlightOfferAvailabilitySourceResolver
{
    IFlightOfferAvailabilitySource? Resolve(FlightSourceKey sourceKey);

    IReadOnlyList<FlightSourceKey> ListConfiguredKeys();
}
