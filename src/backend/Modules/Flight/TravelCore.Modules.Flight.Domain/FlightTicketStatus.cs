namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Per-passenger ticket lifecycle for T006. Voided/Refunded remain R7.
/// </summary>
public enum FlightTicketStatus : short
{
    Pending = 1,
    Issued = 2,
}
