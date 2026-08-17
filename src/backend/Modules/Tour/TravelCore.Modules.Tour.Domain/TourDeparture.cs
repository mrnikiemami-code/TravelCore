using NodaTime;

namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// TourDeparture aggregate root — concrete execution instance of a <see cref="TourProduct"/> (P11-R1 · TC-P11-T001).
/// Invariant: TourProduct ≠ TourDeparture. Product owns reusable definition; Departure owns execution identity.
/// Schedule / capacity / status / flight / hotel / pricing / booking are later P11+ tasks — not owned here yet.
/// Lifecycle-ready: timestamps + product link only (status enum deferred to TC-P11-T004).
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

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    /// <summary>
    /// Creates a Departure execution instance for an existing TourProduct.
    /// Does not copy product content; does not invent schedule/capacity/status.
    /// </summary>
    public static TourDeparture Create(TourProduct product, Instant now)
    {
        ArgumentNullException.ThrowIfNull(product);
        return new TourDeparture(TourDepartureId.New(), product.Id, now);
    }

    /// <summary>Test / reconstitution helper when TourProductId has already been validated.</summary>
    public static TourDeparture Reconstitute(
        TourDepartureId id,
        TourProductId tourProductId,
        Instant createdAt,
        Instant updatedAt)
    {
        return new TourDeparture(id, tourProductId, createdAt)
        {
            UpdatedAt = updatedAt
        };
    }
}
