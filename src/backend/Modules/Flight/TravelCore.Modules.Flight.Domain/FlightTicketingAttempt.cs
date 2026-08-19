using NodaTime;

namespace TravelCore.Modules.Flight.Domain;

/// <summary>
/// One ticketing attempt against the same source that owns the confirmed reservation.
/// Timeout leaves Initiated; Failed requires authoritative "no ticket issued".
/// </summary>
public sealed class FlightTicketingAttempt
{
    private FlightTicketingAttempt()
    {
    }

    internal FlightTicketingAttempt(
        FlightTicketingAttemptId id,
        FlightBookingId flightBookingId,
        Instant createdAt)
    {
        Id = id;
        FlightBookingId = flightBookingId;
        Status = FlightTicketingAttemptStatus.Created;
        CreatedAt = createdAt;
    }

    public static FlightTicketingAttempt StartCreated(FlightBookingId flightBookingId, Instant createdAt)
    {
        if (createdAt == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(createdAt));
        }

        return new FlightTicketingAttempt(FlightTicketingAttemptId.New(), flightBookingId, createdAt);
    }

    public FlightTicketingAttemptId Id { get; private set; }

    public FlightBookingId FlightBookingId { get; private set; }

    public FlightTicketingAttemptStatus Status { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant? InitiatedAt { get; private set; }

    public Instant? SucceededAt { get; private set; }

    public Instant? FailedAt { get; private set; }

    public bool IsUnresolved =>
        Status is FlightTicketingAttemptStatus.Created
            or FlightTicketingAttemptStatus.Initiated;

    public bool IsTerminal =>
        Status is FlightTicketingAttemptStatus.Succeeded
            or FlightTicketingAttemptStatus.Failed;

    public void MarkInitiated(Instant now)
    {
        EnsureClock(now);
        if (Status == FlightTicketingAttemptStatus.Initiated)
        {
            return;
        }

        if (Status != FlightTicketingAttemptStatus.Created)
        {
            throw new InvalidOperationException($"Attempt in status {Status} cannot become Initiated.");
        }

        Status = FlightTicketingAttemptStatus.Initiated;
        InitiatedAt ??= now;
    }

    public void MarkSucceeded(Instant now)
    {
        EnsureClock(now);
        if (Status == FlightTicketingAttemptStatus.Succeeded)
        {
            return;
        }

        if (Status == FlightTicketingAttemptStatus.Failed)
        {
            throw new InvalidOperationException("Failed ticketing attempt cannot become Succeeded.");
        }

        Status = FlightTicketingAttemptStatus.Succeeded;
        SucceededAt = now;
    }

    public void MarkFailed(Instant now)
    {
        EnsureClock(now);
        if (Status == FlightTicketingAttemptStatus.Failed)
        {
            return;
        }

        if (Status == FlightTicketingAttemptStatus.Succeeded)
        {
            throw new InvalidOperationException("Succeeded ticketing attempt cannot become Failed.");
        }

        Status = FlightTicketingAttemptStatus.Failed;
        FailedAt = now;
    }

    private static void EnsureClock(Instant now)
    {
        if (now == default)
        {
            throw new ArgumentException("Timestamp cannot be default.", nameof(now));
        }
    }
}
