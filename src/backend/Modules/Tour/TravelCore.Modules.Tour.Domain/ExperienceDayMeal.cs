namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Meal plan item owned by an <see cref="ExperienceItineraryDay"/> (P10-R5 · TC-P10-T004).
/// </summary>
public sealed class ExperienceDayMeal
{
    private ExperienceDayMeal()
    {
    }

    private ExperienceDayMeal(ExperienceDayMealId id, ItineraryDayId itineraryDayId, ExperienceMealType mealType)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("ExperienceDayMealId cannot be empty.", nameof(id));
        }

        if (itineraryDayId.Value == Guid.Empty)
        {
            throw new ArgumentException("ItineraryDayId cannot be empty.", nameof(itineraryDayId));
        }

        if (!Enum.IsDefined(mealType))
        {
            throw new ArgumentOutOfRangeException(nameof(mealType), mealType, "Unsupported ExperienceMealType.");
        }

        Id = id;
        ItineraryDayId = itineraryDayId;
        MealType = mealType;
    }

    public ExperienceDayMealId Id { get; private set; }

    public ItineraryDayId ItineraryDayId { get; private set; }

    public ExperienceMealType MealType { get; private set; }

    internal static ExperienceDayMeal Create(ExperienceDayMealId id, ItineraryDayId itineraryDayId, ExperienceMealType mealType)
        => new(id, itineraryDayId, mealType);

    public static ExperienceDayMeal Reconstitute(
        ExperienceDayMealId id,
        ItineraryDayId itineraryDayId,
        ExperienceMealType mealType)
        => new(id, itineraryDayId, mealType);
}
