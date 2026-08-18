namespace TravelCore.Modules.HotelBooking.Domain;

public enum HotelBookingReconciliationIssueKind : short
{
    MonetaryMismatch = 1,
    CurrencyMismatch = 2,
    RoomSetMismatch = 3,
    StayMismatch = 4,
    HotelMismatch = 5,
    CancellationTermsMismatch = 6,
    ContradictorySupplierEvidence = 7,
    AmbiguousReservationOutcome = 8,
}
