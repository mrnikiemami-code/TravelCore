namespace TravelCore.Modules.Flight.Contracts;

/// <summary>
/// Internal read-only operational FlightBooking facts (P22-R8).
/// Not a customer API. No secrets, raw payloads, or access tokens.
/// </summary>
public static class FlightOperationalBoundary
{
    public const bool PublicOperationalEndpointImplemented = false;
    public const bool ManualFlightBookingMutationImplemented = false;
    public const string OperationalReadsAreNotTruthAuthority =
        "OperationalRead != FlightBookingTruthAuthority";
    public const string RecheckOutcomeSource = "AuthoritativeSourceQuery";
}

public sealed record FlightOperationalPassengerCountRead(
    int AdultCount,
    int ChildCount,
    int InfantCount);

public sealed record FlightOperationalRead(
    Guid FlightBookingId,
    string TripType,
    string OriginIata,
    string DestinationIata,
    DateTimeOffset DepartureAt,
    FlightOperationalPassengerCountRead Passengers,
    string FlightBookingStatus,
    decimal? AcceptedTotal,
    string? CurrencyCode,
    string? OfferSourceKey,
    string? ReservationPresentationStatus,
    int ReservationAttemptCount,
    string? PaymentStatus,
    string? RefundStatus,
    string? TicketSummary,
    string? CancellationStatus,
    string? ReconciliationSummary,
    string? SourceKey,
    string? SourceReservationReference);

public interface IFlightOperationalQuery
{
    Task<FlightOperationalRead?> GetByFlightBookingIdAsync(
        Guid flightBookingId,
        CancellationToken cancellationToken = default);

    Task<string> RecheckSupplierReservationAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default);

    Task<string> RecheckSupplierCancellationAsync(
        Guid cancellationId,
        CancellationToken cancellationToken = default);
}
