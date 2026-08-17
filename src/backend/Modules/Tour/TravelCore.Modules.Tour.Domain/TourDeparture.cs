using NodaTime;

namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// TourDeparture aggregate root — concrete execution instance of a <see cref="TourProduct"/> (P11-R1 · TC-P11-T001).
/// Invariant: TourProduct ≠ TourDeparture. Product owns reusable definition; Departure owns execution identity.
/// Schedule: <see cref="TourDepartureSchedule"/> (P11-R2 · TC-P11-T002).
/// Capacity rules: <see cref="TourDepartureCapacity"/> (P11-R3 · TC-P11-T003).
/// Lifecycle: <see cref="TourDepartureStatus"/> (P11-R4 · TC-P11-T004) — ≠ CatalogStatus / SEO / Booking.
/// Flight / hotel / pricing / booking later.
/// </summary>
public sealed class TourDeparture
{
    private TourDeparture()
    {
    }

    private TourDeparture(
        TourDepartureId id,
        TourProductId tourProductId,
        Instant createdAt)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("TourDepartureId cannot be empty.", nameof(id));
        }

        if (tourProductId.Value == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        Id = id;
        TourProductId = tourProductId;
        Status = TourDepartureStatus.Draft;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public TourDepartureId Id { get; private set; }

    /// <summary>Owning reusable TourProduct (0..N Departures per product — P11-R1).</summary>
    public TourProductId TourProductId { get; private set; }

    /// <summary>Execution lifecycle status (P11-R4). Not catalog/SEO/booking.</summary>
    public TourDepartureStatus Status { get; private set; }

    /// <summary>Optional schedule until attached (P11-R2).</summary>
    public TourDepartureSchedule? Schedule { get; private set; }

    /// <summary>Optional capacity rules until attached (P11-R3). Not booked/available seats.</summary>
    public TourDepartureCapacity? Capacity { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    /// <summary>
    /// Creates a Departure execution instance for an existing TourProduct (starts as Draft).
    /// </summary>
    public static TourDeparture Create(TourProduct product, Instant now)
    {
        ArgumentNullException.ThrowIfNull(product);
        return new TourDeparture(TourDepartureId.New(), product.Id, now);
    }

    /// <summary>Attaches or replaces the travel-date schedule (LocalDate + IANA zone).</summary>
    public void SetSchedule(LocalDate startDate, LocalDate endDate, string timeZoneId, Instant now)
    {
        Schedule = TourDepartureSchedule.Create(startDate, endDate, timeZoneId);
        UpdatedAt = now;
    }

    /// <summary>Attaches or replaces planned pax capacity rules (not reservation counts).</summary>
    public void SetCapacity(int minimumPax, int maximumPax, Instant now)
    {
        Capacity = TourDepartureCapacity.Create(minimumPax, maximumPax);
        UpdatedAt = now;
    }

    /// <summary>
    /// Transitions execution lifecycle status per P11-R4.
    /// Allowed: Draft→Published, Published→Closed, Published→Cancelled, Closed→Completed.
    /// </summary>
    public void SetStatus(TourDepartureStatus status, Instant now)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported TourDepartureStatus.");
        }

        if (status == Status)
        {
            UpdatedAt = now;
            return;
        }

        if (!IsTransitionAllowed(Status, status))
        {
            throw new InvalidOperationException(
                $"Illegal TourDeparture status transition: {Status} -> {status}.");
        }

        Status = status;
        UpdatedAt = now;
    }

    private static bool IsTransitionAllowed(TourDepartureStatus from, TourDepartureStatus to) =>
        (from, to) switch
        {
            (TourDepartureStatus.Draft, TourDepartureStatus.Published) => true,
            (TourDepartureStatus.Published, TourDepartureStatus.Closed) => true,
            (TourDepartureStatus.Published, TourDepartureStatus.Cancelled) => true,
            (TourDepartureStatus.Closed, TourDepartureStatus.Completed) => true,
            _ => false
        };

    /// <summary>Test / reconstitution helper when TourProductId has already been validated.</summary>
    public static TourDeparture Reconstitute(
        TourDepartureId id,
        TourProductId tourProductId,
        Instant createdAt,
        Instant updatedAt,
        TourDepartureStatus status = TourDepartureStatus.Draft,
        TourDepartureSchedule? schedule = null,
        TourDepartureCapacity? capacity = null)
    {
        return new TourDeparture(id, tourProductId, createdAt)
        {
            Status = status,
            UpdatedAt = updatedAt,
            Schedule = schedule,
            Capacity = capacity
        };
    }
}
