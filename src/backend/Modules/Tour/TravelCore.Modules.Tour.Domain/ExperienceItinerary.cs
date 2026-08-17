using NodaTime;

namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Experience-owned itinerary child aggregate (P10-R1 · TC-P10-T002).
/// PK = <see cref="TourProductId"/> (same as owning <see cref="TourExperienceSpecialization"/>).
/// Cardinality: 0..1 per Experience. Not a standalone business aggregate.
/// Destination/Attraction links · meals · etc. deferred to later tasks.
/// </summary>
public sealed class ExperienceItinerary
{
    public const int MaxDays = 60;
    public const int MaxStopsPerDay = 40;

    private readonly List<ExperienceItineraryDay> _days = [];

    private ExperienceItinerary()
    {
    }

    private ExperienceItinerary(TourProductId tourProductId, Instant createdAt)
    {
        if (tourProductId.Value == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        TourProductId = tourProductId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public TourProductId TourProductId { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public IReadOnlyCollection<ExperienceItineraryDay> Days => _days;

    public IReadOnlyList<ExperienceItineraryDay> DaysOrdered =>
        _days.OrderBy(x => x.DayNumber).ToList();

    internal static ExperienceItinerary Create(TourProductId tourProductId, Instant now)
        => new(tourProductId, now);

    public static ExperienceItinerary Reconstitute(
        TourProductId tourProductId,
        Instant createdAt,
        Instant updatedAt,
        IEnumerable<ExperienceItineraryDay>? days = null)
    {
        var itinerary = new ExperienceItinerary(tourProductId, createdAt)
        {
            UpdatedAt = updatedAt
        };

        if (days is not null)
        {
            foreach (var day in days.OrderBy(x => x.DayNumber))
            {
                itinerary._days.Add(day);
            }
        }

        return itinerary;
    }

    public ExperienceItineraryDay AddDay(int dayNumber, Instant now, ItineraryDayId? id = null)
    {
        if (dayNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(dayNumber), dayNumber, "DayNumber must be >= 1.");
        }

        if (_days.Count >= MaxDays)
        {
            throw new InvalidOperationException($"An itinerary may have at most {MaxDays} days.");
        }

        if (_days.Any(x => x.DayNumber == dayNumber))
        {
            throw new ArgumentException($"DayNumber {dayNumber} is already used on this itinerary.", nameof(dayNumber));
        }

        var day = ExperienceItineraryDay.Create(TourProductId, id ?? ItineraryDayId.New(), dayNumber);
        _days.Add(day);
        UpdatedAt = now;
        return day;
    }

    public ExperienceItineraryDay GetDay(ItineraryDayId dayId)
        => _days.FirstOrDefault(x => x.Id == dayId)
           ?? throw new ArgumentException($"Itinerary day '{dayId}' was not found.", nameof(dayId));

    public ExperienceItineraryStop AddStop(ItineraryDayId dayId, Instant now, int? sortOrder = null, ItineraryStopId? id = null)
    {
        var day = GetDay(dayId);
        var stop = day.AddStop(id ?? ItineraryStopId.New(), sortOrder);
        UpdatedAt = now;
        return stop;
    }

    public ExperienceItineraryStop GetStop(ItineraryStopId stopId)
    {
        foreach (var day in _days)
        {
            var stop = day.Stops.FirstOrDefault(x => x.Id == stopId);
            if (stop is not null)
            {
                return stop;
            }
        }

        throw new ArgumentException($"Itinerary stop '{stopId}' was not found.", nameof(stopId));
    }

    public ExperienceItineraryStop SetStopDestinationLink(ItineraryStopId stopId, Guid? destinationId, Instant now)
    {
        var stop = GetStop(stopId);
        stop.SetDestinationLink(destinationId);
        UpdatedAt = now;
        return stop;
    }

    public ExperienceItineraryStop SetStopPlaceLink(ItineraryStopId stopId, Guid? placeId, Instant now)
    {
        var stop = GetStop(stopId);
        stop.SetPlaceLink(placeId);
        UpdatedAt = now;
        return stop;
    }

    public ExperienceDayMeal AddMeal(ItineraryDayId dayId, ExperienceMealType mealType, Instant now)
    {
        var day = GetDay(dayId);
        var meal = day.AddMeal(mealType);
        UpdatedAt = now;
        return meal;
    }

    public bool RemoveMeal(ItineraryDayId dayId, ExperienceMealType mealType, Instant now)
    {
        var day = GetDay(dayId);
        var removed = day.RemoveMeal(mealType);
        if (removed)
        {
            UpdatedAt = now;
        }

        return removed;
    }

    public bool RemoveDay(ItineraryDayId dayId, Instant now)
    {
        var day = _days.FirstOrDefault(x => x.Id == dayId);
        if (day is null)
        {
            return false;
        }

        _days.Remove(day);
        UpdatedAt = now;
        return true;
    }

    internal void Touch(Instant now) => UpdatedAt = now;
}
