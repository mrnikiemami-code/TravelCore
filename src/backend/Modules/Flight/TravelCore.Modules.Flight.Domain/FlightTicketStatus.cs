namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Per-passenger ticket lifecycle. Voided/Refunded are supplier ticket facts, not Payment Refund.
/// </summary>
public enum FlightTicketStatus : short
{
    Pending = 1,
    Issued = 2,
    Voided = 3,
    Refunded = 4,
}
