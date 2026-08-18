namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Server-controlled commercial-offer source resolution. No ranking, failover, or client-selected adapter.
/// </summary>
public interface IFlightOfferSourceResolver
{
    IFlightOfferSource? Resolve(FlightSourceKey sourceKey);

    IReadOnlyList<FlightSourceKey> ListConfiguredKeys();
}
