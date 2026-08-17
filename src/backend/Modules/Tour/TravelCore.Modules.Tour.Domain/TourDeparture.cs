using NodaTime;

namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// TourDeparture aggregate root — concrete execution instance of a <see cref="TourProduct"/> (P11-R1 · TC-P11-T001).
/// Invariant: TourProduct ≠ TourDeparture. Product owns reusable definition; Departure owns execution identity.
/// Schedule: <see cref="TourDepartureSchedule"/> (P11-R2 · TC-P11-T002). Capacity / status / flight / hotel / pricing / booking later.
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
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public TourDepartureId Id { get; private set; }

    /// <summary>Owning reusable TourProduct (0..N Departures per product — P11-R1).</summary>
    public TourProductId TourProductId { get; private set; }

    /// <summary>Optional schedule until attached (P11-R2).</summary>
    public TourDepartureSchedule? Schedule { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    /// <summary>
    /// Creates a Departure execution instance for an existing TourProduct.
    /// Does not copy product content; does not invent capacity/status.
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

    /// <summary>Test / reconstitution helper when TourProductId has already been validated.</summary>
    public static TourDeparture Reconstitute(
        TourDepartureId id,
        TourProductId tourProductId,
        Instant createdAt,
        Instant updatedAt,
        TourDepartureSchedule? schedule = null)
    {
        return new TourDeparture(id, tourProductId, createdAt)
        {
            UpdatedAt = updatedAt,
            Schedule = schedule
        };
    }
}
