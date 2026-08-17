using NodaTime;

namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Experience typed specialization on TourProduct (P09-R1 · P10-R1 · TC-P10-T001/T002).
/// 1:1 with <see cref="TourProductId"/>. Owns optional <see cref="ExperienceItinerary"/> (0..1).
/// Meals · difficulty · guide · publishing rules remain deferred.
/// Package specialty is out of scope (P11).
/// </summary>
public sealed class TourExperienceSpecialization
{
    private ExperienceItinerary? _itinerary;

    private TourExperienceSpecialization()
    {
    }

    private TourExperienceSpecialization(TourProductId tourProductId, Instant createdAt)
    {
        if (tourProductId.Value == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        TourProductId = tourProductId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    /// <summary>Same identity as the owning Experience <see cref="TourProduct"/> (1:1).</summary>
    public TourProductId TourProductId { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    /// <summary>Optional Experience-owned itinerary (P10-R1 · 0..1).</summary>
    public ExperienceItinerary? Itinerary => _itinerary;

    /// <summary>
    /// Attaches Experience specialization to an Experience-kind TourProduct.
    /// Rejects Package (and any non-Experience kind) — no Package specialty in P10.
    /// </summary>
    public static TourExperienceSpecialization CreateFor(TourProduct product, Instant now)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (product.Kind != TourKind.Experience)
        {
            throw new InvalidOperationException(
                "TourExperienceSpecialization may only attach to TourKind.Experience products. Package specialty is out of P10 scope.");
        }

        return new TourExperienceSpecialization(product.Id, now);
    }

    /// <summary>Test / reconstitution helper when Kind has already been validated.</summary>
    public static TourExperienceSpecialization Reconstitute(
        TourProductId tourProductId,
        Instant createdAt,
        Instant updatedAt,
        ExperienceItinerary? itinerary = null)
    {
        var specialization = new TourExperienceSpecialization(tourProductId, createdAt)
        {
            UpdatedAt = updatedAt
        };
        specialization._itinerary = itinerary;
        return specialization;
    }

    /// <summary>
    /// Creates the Experience-owned itinerary (0..1). Idempotent if already present.
    /// </summary>
    public ExperienceItinerary EnsureItinerary(Instant now)
    {
        if (_itinerary is not null)
        {
            return _itinerary;
        }

        _itinerary = ExperienceItinerary.Create(TourProductId, now);
        UpdatedAt = now;
        return _itinerary;
    }

    public void Touch(Instant now) => UpdatedAt = now;
}
