using NodaTime;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Flight-owned reservation reconciliation evidence. Not FlightBookingStatus and not a ticket queue.
/// </summary>
public sealed class FlightReconciliationIssue
{
    public const int DetailMaxLength = 256;

    private FlightReconciliationIssue()
    {
        Detail = string.Empty;
    }

    public FlightReconciliationIssue(
        FlightBookingId flightBookingId,
        FlightReconciliationIssueKind kind,
        Instant createdAt,
        FlightSupplierReservationId? reservationId = null,
        FlightSupplierReservationAttemptId? attemptId = null,
        string? detail = null)
    {
        if (createdAt == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(createdAt));
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Reconciliation kind is not controlled.");
        }

        Id = FlightReconciliationIssueId.New();
        FlightBookingId = flightBookingId;
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

    public FlightReconciliationIssueId Id { get; private set; }

    public FlightBookingId FlightBookingId { get; private set; }

    public FlightSupplierReservationId? ReservationId { get; private set; }

    public FlightSupplierReservationAttemptId? AttemptId { get; private set; }

    public FlightReconciliationIssueKind Kind { get; private set; }

    public Instant CreatedAt { get; private set; }

    public string Detail { get; private set; }

    public bool BlocksConfirmation =>
        Kind is not FlightReconciliationIssueKind.AmbiguousReservationOutcome
            and not FlightReconciliationIssueKind.ContradictorySupplierEvidence;
}
