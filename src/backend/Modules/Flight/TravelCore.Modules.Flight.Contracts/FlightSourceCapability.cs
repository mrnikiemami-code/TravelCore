namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Explicit Flight source capabilities. Only implemented R3 behavior is declared.
/// Do not infer behavior from SourceKey or a provider name.
/// </summary>
public enum FlightSourceCapability : short
{
    Search = 1,
    AvailabilityCheck = 2,
    OfferRevalidation = 3,
}
