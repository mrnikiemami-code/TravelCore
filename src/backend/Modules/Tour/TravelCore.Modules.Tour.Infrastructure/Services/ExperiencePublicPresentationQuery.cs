using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Tour.Contracts;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Services;

/// <summary>
/// Public Published Experience presentation (TC-P14-T004). Existing Tour facts only.
/// </summary>
public sealed class ExperiencePublicPresentationQuery : IExperiencePublicPresentationQuery
{
    private readonly TourDbContext _db;

    public ExperiencePublicPresentationQuery(TourDbContext db)
    {
        _db = db;
    }

    public async Task<PublishedExperiencePresentation?> GetPublishedAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default)
    {
        if (tourProductId == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        var id = TourProductId.From(tourProductId);
        var product = await _db.TourProducts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null
            || product.Kind != TourKind.Experience
            || product.CatalogStatus != TourCatalogStatus.Published)
        {
            return null;
        }

        var specialization = await _db.ExperienceSpecializations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TourProductId == id, cancellationToken);
        if (specialization is null)
        {
            return new PublishedExperiencePresentation(
                tourProductId,
                Difficulty: null,
                ItineraryDays: [],
                Eligibility: [],
                Equipment: [],
                LocalTransport: [],
                Guides: [],
                AccommodationPlan: []);
        }

        var days = specialization.Itinerary?.DaysOrdered
            .Select(day => new PublishedExperienceItineraryDay(
                day.DayNumber,
                day.StopsOrdered
                    .Select(stop => new PublishedExperienceStop(
                        stop.SortOrder,
                        stop.DestinationId,
                        stop.PlaceId))
                    .ToList(),
                day.MealsOrdered.Select(meal => meal.MealType.ToString()).ToList()))
            .ToList()
            ?? [];

        return new PublishedExperiencePresentation(
            tourProductId,
            specialization.Difficulty?.ToString(),
            days,
            specialization.EligibilityRequirements
                .OrderBy(x => x.Code, StringComparer.Ordinal)
                .Select(x => new PublishedExperienceFact(x.Code, x.Value, x.Detail))
                .ToList(),
            specialization.Equipment
                .OrderBy(x => x.Code, StringComparer.Ordinal)
                .Select(x => new PublishedExperienceEquipment(x.Code, x.Kind.ToString(), x.Detail))
                .ToList(),
            specialization.LocalTransport
                .OrderBy(x => x.Code, StringComparer.Ordinal)
                .Select(x => new PublishedExperienceFact(x.Code, null, x.Detail))
                .ToList(),
            specialization.GuideAssignments
                .OrderBy(x => x.Id.Value)
                .Select(x => new PublishedExperienceGuide(x.GuidePartyId, x.Role.ToString(), x.Note))
                .ToList(),
            specialization.AccommodationPlanOrdered
                .Select(x => new PublishedExperienceStay(x.SortOrder, x.PlaceId))
                .ToList());
    }
}
