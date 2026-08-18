namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Baseline trip types for P22-R2. MultiCity remains DEFERRED.
/// </summary>
public enum FlightTripType : short
{
    OneWay = 1,
    RoundTrip = 2,
}
