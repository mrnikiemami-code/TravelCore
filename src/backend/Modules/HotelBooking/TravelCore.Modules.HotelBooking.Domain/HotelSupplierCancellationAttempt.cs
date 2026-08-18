using NodaTime;

namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Durable supplier-cancellation attempt. Network timeout remains Initiated, not Failed.
/// </summary>
public sealed class HotelSupplierCancellationAttempt
{
    private HotelSupplierCancellationAttempt()
    {
    }

    internal HotelSupplierCancellationAttempt(
        HotelSupplierCancellationAttemptId id,
        HotelBookingCancellationId cancellationId,
        Instant createdAt)
    {
        Id = id;
        CancellationId = cancellationId;
        Status = HotelSupplierCancellationAttemptStatus.Created;
        CreatedAt = createdAt;
    }

    public HotelSupplierCancellationAttemptId Id { get; private set; }

    public HotelBookingCancellationId CancellationId { get; private set; }

    public HotelSupplierCancellationAttemptStatus Status { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant? InitiatedAt { get; private set; }

    public Instant? ConfirmedAt { get; private set; }

    public Instant? FailedAt { get; private set; }

    public bool IsUnresolved =>
        Status is HotelSupplierCancellationAttemptStatus.Created
            or HotelSupplierCancellationAttemptStatus.Initiated;

    public bool IsTerminal =>
        Status is HotelSupplierCancellationAttemptStatus.Confirmed
            or HotelSupplierCancellationAttemptStatus.Failed;

    internal void MarkInitiated(Instant now)
    {
        EnsureClock(now);
        if (Status == HotelSupplierCancellationAttemptStatus.Initiated)
        {
            return;
        }

        if (Status != HotelSupplierCancellationAttemptStatus.Created)
        {
            throw new InvalidOperationException($"Attempt in status {Status} cannot become Initiated.");
        }

        Status = HotelSupplierCancellationAttemptStatus.Initiated;
        InitiatedAt ??= now;
    }

    internal void MarkConfirmed(Instant now)
    {
        EnsureClock(now);
        if (Status == HotelSupplierCancellationAttemptStatus.Confirmed)
        {
            return;
        }

        if (Status == HotelSupplierCancellationAttemptStatus.Failed)
        {
            throw new InvalidOperationException("Failed attempt cannot become Confirmed.");
        }

        Status = HotelSupplierCancellationAttemptStatus.Confirmed;
        ConfirmedAt = now;
    }

    internal void MarkFailed(Instant now)
    {
        EnsureClock(now);
        if (Status == HotelSupplierCancellationAttemptStatus.Failed)
        {
            return;
        }

        if (Status == HotelSupplierCancellationAttemptStatus.Confirmed)
        {
            throw new InvalidOperationException("Confirmed attempt cannot become Failed.");
        }

        Status = HotelSupplierCancellationAttemptStatus.Failed;
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
