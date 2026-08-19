namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Explicit Flight source capabilities. Do not infer behavior from SourceKey or a provider name.
/// Ticket/Cancel/Refund capabilities are out of T005 scope.
/// </summary>
public enum FlightSourceCapability : short
{
    Search = 1,
    AvailabilityCheck = 2,
    OfferRevalidation = 3,
    ReservationCreate = 4,
    ReservationQuery = 5,
    TicketCreate = 6,
    TicketQuery = 7,
}
