namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Durable supplier reversal attempt states. Timeout/Unknown remains Initiated, not Failed.
/// </summary>
public enum FlightSupplierReversalAttemptStatus : short
{
    Created = 1,
    Initiated = 2,
    Succeeded = 3,
    Failed = 4,
}
