namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Provider-neutral authoritative Flight commercial-offer port. Named production source remains NONE.
/// Distinct from search and from live availability check. Not a giant supplier gateway.
/// </summary>
public interface IFlightOfferSource
{
    FlightSourceKey Key { get; }

    IReadOnlySet<FlightSourceCapability> Capabilities { get; }

    Task<FlightOfferSourceResult> GetOfferAsync(
        FlightOfferRequest request,
        CancellationToken cancellationToken = default);
}
