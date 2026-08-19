namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Supplier reversal kind for confirmed Flight cancellation. Ticket void/refund is not Payment Refund.
/// </summary>
public enum FlightSupplierReversalKind : short
{
    TicketVoid = 1,
    TicketRefund = 2,
    ReservationCancel = 3,
}
