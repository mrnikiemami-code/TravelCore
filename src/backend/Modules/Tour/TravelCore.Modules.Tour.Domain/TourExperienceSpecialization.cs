using NodaTime;

namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Experience typed specialization foundation on TourProduct (P09-R1 · TC-P10-T001).
/// 1:1 with <see cref="TourProductId"/> — identity/marker only in T001.
/// Itinerary · Day · Stop · meals · difficulty · guide · publishing rules are deferred
/// (P10-R1 itinerary ownership remains open for later tasks; do not invent here).
/// Package specialty is out of scope (P11).
/// </summary>
public sealed class TourExperienceSpecialization
{
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
        Instant updatedAt)
    {
        return new TourExperienceSpecialization(tourProductId, createdAt)
        {
            UpdatedAt = updatedAt
        };
    }

    public void Touch(Instant now) => UpdatedAt = now;
}
