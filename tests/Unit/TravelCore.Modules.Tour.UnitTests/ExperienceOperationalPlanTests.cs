using NodaTime;
using TravelCore.Modules.Tour.Domain;
using Xunit;

namespace TravelCore.Modules.Tour.UnitTests;

public sealed class ExperienceOperationalPlanTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 6, 0);

    [Fact]
    public void DayMeals_BelongToDay_UniqueMealType()
    {
        var product = TourProduct.CreateExperience("EXP-MEAL-001", "Walk", Now);
        var spec = TourExperienceSpecialization.CreateFor(product, Now);
        var itinerary = spec.EnsureItinerary(Now);
        var day = itinerary.AddDay(1, Now);

        itinerary.AddMeal(day.Id, ExperienceMealType.Breakfast, Now);
        itinerary.AddMeal(day.Id, ExperienceMealType.Dinner, Now);

        Assert.Equal(2, day.Meals.Count);
        Assert.Throws<ArgumentException>(() => itinerary.AddMeal(day.Id, ExperienceMealType.Breakfast, Now));
        Assert.True(itinerary.RemoveMeal(day.Id, ExperienceMealType.Breakfast, Now));
        Assert.Single(day.Meals);
    }

    [Fact]
    public void AccommodationPlan_IsZeroToN_WithOptionalPlaceId()
    {
        var product = TourProduct.CreateExperience("EXP-ACC-001", "Walk", Now);
        var spec = TourExperienceSpecialization.CreateFor(product, Now);
        var hotelId = Guid.Parse("01900000-0000-7000-8000-000000000801");

        var a = spec.AddAccommodationPlanEntry(Now, placeId: hotelId);
        var b = spec.AddAccommodationPlanEntry(Now, placeId: null);

        Assert.Equal(0, a.SortOrder);
        Assert.Equal(1, b.SortOrder);
        Assert.Equal(hotelId, a.PlaceId);
        Assert.Null(b.PlaceId);
        Assert.Equal(2, spec.AccommodationPlan.Count);
        Assert.Throws<ArgumentException>(() => a.SetPlaceLink(Guid.Empty));
        Assert.True(spec.RemoveAccommodationPlanEntry(a.Id, Now));
        Assert.Single(spec.AccommodationPlan);
    }
}
