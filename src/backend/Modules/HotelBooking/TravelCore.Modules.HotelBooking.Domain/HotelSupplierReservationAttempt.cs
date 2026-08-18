using NodaTime;

namespace TravelCore.Modules.HotelBooking.Domain;

public sealed class HotelSupplierReservationAttempt
{
    private HotelSupplierReservationAttempt()
    {
    }

    internal HotelSupplierReservationAttempt(
        HotelSupplierReservationAttemptId id,
        HotelSupplierReservationId reservationId,
        Instant createdAt)
    {
        Id = id;
        ReservationId = reservationId;
        Status = HotelSupplierReservationAttemptStatus.Created;
        CreatedAt = createdAt;
    }

    public HotelSupplierReservationAttemptId Id { get; private set; }

    public HotelSupplierReservationId ReservationId { get; private set; }

    public HotelSupplierReservationAttemptStatus Status { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant? InitiatedAt { get; private set; }

    public Instant? ConfirmedAt { get; private set; }

    public Instant? FailedAt { get; private set; }

    public bool IsUnresolved =>
        Status is HotelSupplierReservationAttemptStatus.Created
            or HotelSupplierReservationAttemptStatus.Initiated;

    public bool IsTerminal =>
        Status is HotelSupplierReservationAttemptStatus.Confirmed
            or HotelSupplierReservationAttemptStatus.Failed;

    internal void MarkInitiated(Instant now)
    {
        EnsureClock(now);
        if (Status == HotelSupplierReservationAttemptStatus.Initiated)
        {
            return;
        }

        if (Status != HotelSupplierReservationAttemptStatus.Created
            && Status != HotelSupplierReservationAttemptStatus.Initiated)
        {
            throw new InvalidOperationException($"Attempt in status {Status} cannot become Initiated.");
        }

        Status = HotelSupplierReservationAttemptStatus.Initiated;
        InitiatedAt ??= now;
    }

    internal void MarkConfirmed(Instant now)
    {
        EnsureClock(now);
        if (Status == HotelSupplierReservationAttemptStatus.Confirmed)
        {
            return;
        }

        if (Status == HotelSupplierReservationAttemptStatus.Failed)
        {
            throw new InvalidOperationException("Failed attempt cannot become Confirmed.");
        }

        Status = HotelSupplierReservationAttemptStatus.Confirmed;
        ConfirmedAt = now;
    }

    internal void MarkFailed(Instant now)
    {
        EnsureClock(now);
        if (Status == HotelSupplierReservationAttemptStatus.Failed)
        {
            return;
        }

        if (Status == HotelSupplierReservationAttemptStatus.Confirmed)
        {
            throw new InvalidOperationException("Confirmed attempt cannot become Failed.");
        }

        Status = HotelSupplierReservationAttemptStatus.Failed;
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
