using NodaTime;

namespace TravelCore.Modules.Flight.Domain;

public sealed class FlightSupplierReservationAttempt
{
    private FlightSupplierReservationAttempt()
    {
    }

    internal FlightSupplierReservationAttempt(
        FlightSupplierReservationAttemptId id,
        FlightSupplierReservationId reservationId,
        Instant createdAt)
    {
        Id = id;
        ReservationId = reservationId;
        Status = FlightSupplierReservationAttemptStatus.Created;
        CreatedAt = createdAt;
    }

    public FlightSupplierReservationAttemptId Id { get; private set; }

    public FlightSupplierReservationId ReservationId { get; private set; }

    public FlightSupplierReservationAttemptStatus Status { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant? InitiatedAt { get; private set; }

    public Instant? ConfirmedAt { get; private set; }

    public Instant? FailedAt { get; private set; }

    public bool IsUnresolved =>
        Status is FlightSupplierReservationAttemptStatus.Created
            or FlightSupplierReservationAttemptStatus.Initiated;

    public bool IsTerminal =>
        Status is FlightSupplierReservationAttemptStatus.Confirmed
            or FlightSupplierReservationAttemptStatus.Failed;

    internal void MarkInitiated(Instant now)
    {
        EnsureClock(now);
        if (Status == FlightSupplierReservationAttemptStatus.Initiated)
        {
            return;
        }

        if (Status != FlightSupplierReservationAttemptStatus.Created)
        {
            throw new InvalidOperationException($"Attempt in status {Status} cannot become Initiated.");
        }

        Status = FlightSupplierReservationAttemptStatus.Initiated;
        InitiatedAt ??= now;
    }

    internal void MarkConfirmed(Instant now)
    {
        EnsureClock(now);
        if (Status == FlightSupplierReservationAttemptStatus.Confirmed)
        {
            return;
        }

        if (Status == FlightSupplierReservationAttemptStatus.Failed)
        {
            throw new InvalidOperationException("Failed attempt cannot become Confirmed.");
        }

        Status = FlightSupplierReservationAttemptStatus.Confirmed;
        ConfirmedAt = now;
    }

    internal void MarkFailed(Instant now)
    {
        EnsureClock(now);
        if (Status == FlightSupplierReservationAttemptStatus.Failed)
        {
            return;
        }

        if (Status == FlightSupplierReservationAttemptStatus.Confirmed)
        {
            throw new InvalidOperationException("Confirmed attempt cannot become Failed.");
        }

        Status = FlightSupplierReservationAttemptStatus.Failed;
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
