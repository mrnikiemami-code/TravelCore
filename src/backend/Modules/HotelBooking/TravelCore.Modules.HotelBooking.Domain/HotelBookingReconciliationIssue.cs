using NodaTime;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// HotelBooking-owned reservation reconciliation evidence. Not a HotelBookingStatus and not a ticket queue.
/// </summary>
public sealed class HotelBookingReconciliationIssue
{
    public const int DetailMaxLength = 256;

    private HotelBookingReconciliationIssue()
    {
        Detail = string.Empty;
    }

    public HotelBookingReconciliationIssue(
        HotelBookingId hotelBookingId,
        HotelBookingReconciliationIssueKind kind,
        Instant createdAt,
        HotelSupplierReservationId? reservationId = null,
        HotelSupplierReservationAttemptId? attemptId = null,
        string? detail = null)
    {
        if (createdAt == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(createdAt));
        }

        Id = HotelBookingReconciliationIssueId.New();
        HotelBookingId = hotelBookingId;
        Kind = kind;
        CreatedAt = createdAt;
        ReservationId = reservationId;
        AttemptId = attemptId;
        Detail = string.IsNullOrWhiteSpace(detail) ? string.Empty : detail.Trim();
        if (Detail.Length > DetailMaxLength)
        {
            Detail = Detail[..DetailMaxLength];
        }
    }

    public HotelBookingReconciliationIssueId Id { get; private set; }

    public HotelBookingId HotelBookingId { get; private set; }

    public HotelSupplierReservationId? ReservationId { get; private set; }

    public HotelSupplierReservationAttemptId? AttemptId { get; private set; }

    public HotelBookingReconciliationIssueKind Kind { get; private set; }

    public Instant CreatedAt { get; private set; }

    public string Detail { get; private set; }

    public bool BlocksConfirmation =>
        Kind is not HotelBookingReconciliationIssueKind.AmbiguousReservationOutcome
            and not HotelBookingReconciliationIssueKind.SupplierCancellationAmbiguous
            and not HotelBookingReconciliationIssueKind.SupplierCancellationContradiction
            and not HotelBookingReconciliationIssueKind.SupplierCancellationEconomicsMismatch
            and not HotelBookingReconciliationIssueKind.MissingPaymentEvidence
            and not HotelBookingReconciliationIssueKind.RefundInvariantMismatch;
}
