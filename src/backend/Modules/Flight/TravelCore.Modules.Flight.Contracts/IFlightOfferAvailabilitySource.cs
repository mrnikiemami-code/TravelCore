namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Authoritative live validation of a selected Flight source option.
/// Not a hold, PNR, ticket, or accepted fare snapshot.
/// </summary>
public interface IFlightOfferAvailabilitySource
{
    FlightSourceKey Key { get; }

    IReadOnlySet<FlightSourceCapability> Capabilities { get; }

    Task<FlightOfferAvailabilityResult> CheckAvailabilityAsync(
        FlightOfferAvailabilityRequest request,
        CancellationToken cancellationToken = default);
}
