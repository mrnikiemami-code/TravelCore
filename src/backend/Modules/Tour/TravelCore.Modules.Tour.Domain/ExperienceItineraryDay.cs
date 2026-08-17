namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Ordered day under an <see cref="ExperienceItinerary"/> (P10-R1 · TC-P10-T002/T004).
/// Owns meal plan items (P10-R5). Localized titles deferred.
/// </summary>
public sealed class ExperienceItineraryDay
{
    public const int MaxMealsPerDay = 8;

    private readonly List<ExperienceItineraryStop> _stops = [];
    private readonly List<ExperienceDayMeal> _meals = [];

    private ExperienceItineraryDay()
    {
    }

    private ExperienceItineraryDay(ItineraryDayId id, TourProductId tourProductId, int dayNumber)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("ItineraryDayId cannot be empty.", nameof(id));
        }

        if (tourProductId.Value == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        if (dayNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(dayNumber), dayNumber, "DayNumber must be >= 1.");
        }

        Id = id;
        TourProductId = tourProductId;
        DayNumber = dayNumber;
    }

    public ItineraryDayId Id { get; private set; }

    /// <summary>Owning itinerary / Experience TourProductId (P10-R1).</summary>
    public TourProductId TourProductId { get; private set; }

    public int DayNumber { get; private set; }

    public IReadOnlyCollection<ExperienceItineraryStop> Stops => _stops;

    public IReadOnlyList<ExperienceItineraryStop> StopsOrdered =>
        _stops.OrderBy(x => x.SortOrder).ToList();

    public IReadOnlyCollection<ExperienceDayMeal> Meals => _meals;

    public IReadOnlyList<ExperienceDayMeal> MealsOrdered =>
        _meals.OrderBy(x => x.MealType).ToList();

    internal static ExperienceItineraryDay Create(TourProductId tourProductId, ItineraryDayId id, int dayNumber)
        => new(id, tourProductId, dayNumber);

    public static ExperienceItineraryDay Reconstitute(
        ItineraryDayId id,
        TourProductId tourProductId,
        int dayNumber,
        IEnumerable<ExperienceItineraryStop>? stops = null,
        IEnumerable<ExperienceDayMeal>? meals = null)
    {
        var day = new ExperienceItineraryDay(id, tourProductId, dayNumber);
        if (stops is not null)
        {
            foreach (var stop in stops.OrderBy(x => x.SortOrder))
            {
                day._stops.Add(stop);
            }
        }

        if (meals is not null)
        {
            foreach (var meal in meals.OrderBy(x => x.MealType))
            {
                day._meals.Add(meal);
            }
        }

        return day;
    }

    internal ExperienceItineraryStop AddStop(ItineraryStopId id, int? sortOrder)
    {
        if (_stops.Count >= ExperienceItinerary.MaxStopsPerDay)
        {
            throw new InvalidOperationException(
                $"An itinerary day may have at most {ExperienceItinerary.MaxStopsPerDay} stops.");
        }

        var resolvedSort = sortOrder ?? NextSortOrder();
        if (resolvedSort < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), resolvedSort, "Stop SortOrder must be >= 0.");
        }

        if (_stops.Any(x => x.SortOrder == resolvedSort))
        {
            throw new ArgumentException(
                $"Stop SortOrder {resolvedSort} is already used on this day.",
                nameof(sortOrder));
        }

        var stop = ExperienceItineraryStop.Create(id, Id, resolvedSort);
        _stops.Add(stop);
        return stop;
    }

    public bool RemoveStop(ItineraryStopId stopId)
    {
        var stop = _stops.FirstOrDefault(x => x.Id == stopId);
        if (stop is null)
        {
            return false;
        }

        _stops.Remove(stop);
        return true;
    }

    internal ExperienceDayMeal AddMeal(ExperienceMealType mealType, ExperienceDayMealId? id = null)
    {
        if (_meals.Count >= MaxMealsPerDay)
        {
            throw new InvalidOperationException($"An itinerary day may have at most {MaxMealsPerDay} meals.");
        }

        if (_meals.Any(x => x.MealType == mealType))
        {
            throw new ArgumentException(
                $"MealType '{mealType}' is already set for this day.",
                nameof(mealType));
        }

        var meal = ExperienceDayMeal.Create(id ?? ExperienceDayMealId.New(), Id, mealType);
        _meals.Add(meal);
        return meal;
    }

    public bool RemoveMeal(ExperienceMealType mealType)
    {
        var meal = _meals.FirstOrDefault(x => x.MealType == mealType);
        if (meal is null)
        {
            return false;
        }

        _meals.Remove(meal);
        return true;
    }

    private int NextSortOrder()
    {
        if (_stops.Count == 0)
        {
            return 0;
        }

        return _stops.Max(x => x.SortOrder) + 1;
    }
}
