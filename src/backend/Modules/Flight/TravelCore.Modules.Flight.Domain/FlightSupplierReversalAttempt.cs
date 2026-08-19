using NodaTime;
using TravelCore.Modules.Flight.Contracts;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// Durable supplier ticket/reservation reversal attempt. Network timeout remains Initiated, not Failed.
/// Ticket attempts store TicketId and PassengerId. Reservation cancel has neither.
/// </summary>
public sealed class FlightSupplierReversalAttempt
{
    private FlightSupplierReversalAttempt()
    {
    }

    internal FlightSupplierReversalAttempt(
        FlightSupplierReversalAttemptId id,
        FlightBookingCancellationId cancellationId,
        FlightSupplierReversalKind kind,
        Instant createdAt,
        FlightTicketId? ticketId,
        FlightPassengerId? passengerId)
    {
        if (kind is FlightSupplierReversalKind.TicketVoid or FlightSupplierReversalKind.TicketRefund)
        {
            if (ticketId is null || passengerId is null)
            {
                throw new ArgumentException("Ticket reversal attempts must store TicketId and PassengerId.");
            }
        }
        else if (kind == FlightSupplierReversalKind.ReservationCancel)
        {
            if (ticketId is not null || passengerId is not null)
            {
                throw new ArgumentException("Reservation cancel attempts cannot store a ticket identity.");
            }
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Reversal kind is not controlled.");
        }

        Id = id;
        CancellationId = cancellationId;
        Kind = kind;
        TicketId = ticketId;
        PassengerId = passengerId;
        Status = FlightSupplierReversalAttemptStatus.Created;
        CreatedAt = createdAt;
    }

    public FlightSupplierReversalAttemptId Id { get; private set; }

    public FlightBookingCancellationId CancellationId { get; private set; }

    public FlightSupplierReversalKind Kind { get; private set; }

    public FlightTicketId? TicketId { get; private set; }

    public FlightPassengerId? PassengerId { get; private set; }

    public FlightSupplierReversalAttemptStatus Status { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant? InitiatedAt { get; private set; }

    public Instant? SucceededAt { get; private set; }

    public Instant? FailedAt { get; private set; }

    public bool IsUnresolved =>
        Status is FlightSupplierReversalAttemptStatus.Created
            or FlightSupplierReversalAttemptStatus.Initiated;

    public bool IsTerminal =>
        Status is FlightSupplierReversalAttemptStatus.Succeeded
            or FlightSupplierReversalAttemptStatus.Failed;

    internal void MarkInitiated(Instant now)
    {
        EnsureClock(now);
        if (Status == FlightSupplierReversalAttemptStatus.Initiated)
        {
            return;
        }

        if (Status != FlightSupplierReversalAttemptStatus.Created)
        {
            throw new InvalidOperationException($"Attempt in status {Status} cannot become Initiated.");
        }

        Status = FlightSupplierReversalAttemptStatus.Initiated;
        InitiatedAt ??= now;
    }

    internal void MarkSucceeded(Instant now)
    {
        EnsureClock(now);
        if (Status == FlightSupplierReversalAttemptStatus.Succeeded)
        {
            return;
        }

        if (Status == FlightSupplierReversalAttemptStatus.Failed)
        {
            throw new InvalidOperationException("Failed attempt cannot become Succeeded.");
        }

        Status = FlightSupplierReversalAttemptStatus.Succeeded;
        SucceededAt = now;
    }

    internal void MarkFailed(Instant now)
    {
        EnsureClock(now);
        if (Status == FlightSupplierReversalAttemptStatus.Failed)
        {
            return;
        }

        if (Status == FlightSupplierReversalAttemptStatus.Succeeded)
        {
            throw new InvalidOperationException("Succeeded attempt cannot become Failed.");
        }

        Status = FlightSupplierReversalAttemptStatus.Failed;
        FailedAt = now;
    }

    private static void EnsureClock(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("Instant cannot be default.", nameof(now));
        }
    }
}
