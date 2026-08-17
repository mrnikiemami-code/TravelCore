namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Experience catalog publishability rules (P10-R8 · TC-P10-T008).
/// Reuses <see cref="TourCatalogStatus"/> on TourProduct — no second status source.
/// Published = catalog visibility only (≠ bookable / priced / available).
/// </summary>
public static class ExperiencePublishability
{
    public static bool HasLocalizedTitle(TourProduct product)
    {
        ArgumentNullException.ThrowIfNull(product);
        return product.Translations.Any(t => !string.IsNullOrWhiteSpace(t.Title));
    }

    public static bool HasCover(TourProduct product)
    {
        ArgumentNullException.ThrowIfNull(product);
        return product.Cover is not null;
    }

    public static bool HasDestination(TourProduct product)
    {
        ArgumentNullException.ThrowIfNull(product);
        return product.Destinations.Count > 0;
    }

    public static bool HasMeaningfulExperienceFacts(
        TourProduct product,
        TourExperienceSpecialization? specialization)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (specialization is null || specialization.TourProductId != product.Id)
        {
            return false;
        }

        if (specialization.Itinerary?.Days.Count > 0)
        {
            return true;
        }

        if (specialization.Difficulty is not null)
        {
            return true;
        }

        if (specialization.EligibilityRequirements.Count > 0
            || specialization.Equipment.Count > 0
            || specialization.LocalTransport.Count > 0
            || specialization.GuideAssignments.Count > 0
            || specialization.AccommodationPlan.Count > 0)
        {
            return true;
        }

        return false;
    }

    public static IReadOnlyList<string> EvaluateBlockingReasons(
        TourProduct product,
        TourExperienceSpecialization? specialization)
    {
        ArgumentNullException.ThrowIfNull(product);

        var reasons = new List<string>();
        if (!HasLocalizedTitle(product))
        {
            reasons.Add("Localized title is required before publishing an Experience.");
        }

        if (!HasCover(product))
        {
            reasons.Add("Cover image is required before publishing an Experience.");
        }

        if (!HasDestination(product))
        {
            reasons.Add("At least one Destination link is required before publishing an Experience.");
        }

        if (!HasMeaningfulExperienceFacts(product, specialization))
        {
            reasons.Add(
                "Meaningful Experience facts are required (itinerary day(s) and/or operational attributes) before publishing.");
        }

        return reasons;
    }

    public static void EnsureCanPublish(TourProduct product, TourExperienceSpecialization? specialization)
    {
        if (product.Kind != TourKind.Experience)
        {
            throw new InvalidOperationException(
                $"Experience publishability applies only to TourKind.Experience (found '{product.Kind}').");
        }

        var reasons = EvaluateBlockingReasons(product, specialization);
        if (reasons.Count > 0)
        {
            throw new InvalidOperationException(
                "Experience is not catalog-complete for Published status: " + string.Join(" ", reasons));
        }
    }
}
