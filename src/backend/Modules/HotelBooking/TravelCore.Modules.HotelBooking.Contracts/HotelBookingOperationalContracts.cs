namespace TravelCore.Modules.HotelBooking.Contracts;

/// <summary>
/// Internal read-only operational HotelBooking facts (P21-R8).
/// Not a customer API. No secrets, raw payloads, or access tokens.
/// </summary>
public static class HotelBookingOperationalBoundary
{
    public const bool PublicOperationalEndpointImplemented = false;
    public const bool ManualHotelBookingMutationImplemented = false;
    public const string OperationalReadsAreNotTruthAuthority =
        "OperationalRead != HotelBookingTruthAuthority";
    public const string RecheckOutcomeSource = "AuthoritativeSourceQuery";
}

public sealed record HotelBookingOperationalOccupancyRead(
    int RoomCount,
    int AdultCount,
    int ChildCount);

public sealed record HotelBookingOperationalRead(
    Guid HotelBookingId,
    Guid PlaceId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    HotelBookingOperationalOccupancyRead Occupancy,
    string HotelBookingStatus,
    decimal? AcceptedTotal,
    string? CurrencyCode,
    string? HoldStatus,
    string? ReservationStatus,
    int ReservationAttemptCount,
    string? PaymentStatus,
    string? RefundStatus,
    string? CancellationStatus,
    string? ReconciliationSummary,
    string? SourceKey,
    string? SourceReservationReference);

public interface IHotelBookingOperationalQuery
{
    Task<HotelBookingOperationalRead?> GetByHotelBookingIdAsync(
        Guid hotelBookingId,
        CancellationToken cancellationToken = default);

    Task<string> RecheckAvailabilityHoldAsync(
        Guid holdId,
        CancellationToken cancellationToken = default);

    Task<string> RecheckSupplierReservationAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default);

    Task<string> RecheckSupplierCancellationAsync(
        Guid cancellationId,
        CancellationToken cancellationToken = default);
}
