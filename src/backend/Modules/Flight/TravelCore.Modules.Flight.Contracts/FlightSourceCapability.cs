namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Explicit Flight source capabilities. Do not infer behavior from SourceKey or a provider name.
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
    CancellationQuote = 8,
    ReservationCancel = 9,
    TicketVoid = 10,
    TicketRefund = 11,
    CancellationQuery = 12,
}
