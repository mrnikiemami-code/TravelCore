namespace TravelCore.Modules.Flight.Domain;

public enum FlightReconciliationIssueKind : short
{
    MonetaryMismatch = 1,
    CurrencyMismatch = 2,
    ItineraryMismatch = 3,
    PassengerMismatch = 4,
    OfferMismatch = 5,
    AmbiguousReservationOutcome = 6,
    ContradictorySupplierEvidence = 7,
    TicketSetMismatch = 8,
    PaymentEvidenceMismatch = 9,
    PartialTicketReversal = 10,
    SupplierCancellationAmbiguous = 11,
    SupplierEconomicsMismatch = 12,
    TicketStillActive = 13,
}
