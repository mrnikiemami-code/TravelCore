using NodaTime;
using NodaTime.Text;
using TravelCore.Money;
using TravelCore.Modules.TripPlanner.Contracts;
using TravelCore.Modules.TripPlanner.Domain;

namespace TravelCore.Modules.TripPlanner.Infrastructure.Services;

internal static class TripPlannerPublicPreferenceMapper
{
    internal static TripPlannerPreferencesDraft ToDraft(TravelPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        var destinations = preferences.Destinations.ToList();
        var undecided = destinations.Count == 0 || destinations.All(d => d.IsUndecided);
        var logicalIds = destinations
            .Where(d => !d.IsUndecided && d.LogicalDestinationId.HasValue)
            .Select(d => d.LogicalDestinationId!.Value)
            .ToList();

        return new TripPlannerPreferencesDraft(
            ToTimingDraft(preferences.Timing),
            preferences.Travelers is null
                ? null
                : new TripPlannerTravelersDraft(
                    preferences.Travelers.AdultCount,
                    preferences.Travelers.ChildCount,
                    preferences.Travelers.InfantCount),
            preferences.Budget is null
                ? null
                : new TripPlannerBudgetDraft(
                    preferences.Budget.MinimumAmount,
                    preferences.Budget.MaximumAmount,
                    preferences.Budget.CurrencyCode.Value),
            preferences.Accommodation?.ToString(),
            preferences.Transport?.ToString(),
            preferences.TripStyle?.ToString(),
            preferences.Interests.Select(i => i.Code).ToList(),
            new TripPlannerDestinationDraft(undecided, logicalIds),
            preferences.TravelerNote);
    }

    internal static void ApplyUpdate(TravelPreferences preferences, TripPlannerUpdateIntentRequest request)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        ArgumentNullException.ThrowIfNull(request);

        if (request.Timing is not null)
        {
            preferences.SetTiming(MapTiming(request.Timing));
        }

        if (request.Travelers is not null)
        {
            preferences.SetTravelers(
                PlannerTravelerComposition.Create(
                    request.Travelers.AdultCount,
                    request.Travelers.ChildCount,
                    request.Travelers.InfantCount));
        }

        if (request.Budget is not null)
        {
            preferences.SetBudget(MapBudget(request.Budget));
        }

        if (request.Accommodation is not null)
        {
            preferences.SetAccommodation(ParseEnum<AccommodationPreferenceKind>(request.Accommodation, nameof(request.Accommodation)));
        }

        if (request.Transport is not null)
        {
            preferences.SetTransport(ParseEnum<TransportPreferenceKind>(request.Transport, nameof(request.Transport)));
        }

        if (request.TripStyle is not null)
        {
            preferences.SetTripStyle(ParseEnum<TripStylePreference>(request.TripStyle, nameof(request.TripStyle)));
        }

        if (request.InterestCodes is not null)
        {
            preferences.ReplaceInterests(request.InterestCodes.Select(InterestPreference.Create));
        }

        if (request.Destination is not null)
        {
            preferences.ReplaceDestinations(MapDestinations(request.Destination));
        }

        if (request.TravelerNote is not null)
        {
            preferences.SetTravelerNote(request.TravelerNote);
        }
    }

    private static TripPlannerTimingDraft ToTimingDraft(TravelTimingPreference timing)
    {
        return new TripPlannerTimingDraft(
            timing.Kind.ToString(),
            timing.ExactStartDate?.ToString("yyyy-MM-dd", null),
            timing.ExactEndDate?.ToString("yyyy-MM-dd", null),
            timing.FlexibleEarliestStart?.ToString("yyyy-MM-dd", null),
            timing.FlexibleLatestStart?.ToString("yyyy-MM-dd", null),
            timing.FlexibleMaxTripDurationDays,
            timing.ApproximateYear,
            timing.ApproximateMonth,
            timing.ApproximateSeason?.ToString());
    }

    private static TravelTimingPreference MapTiming(TripPlannerTimingDraft timing)
    {
        ArgumentNullException.ThrowIfNull(timing);
        if (!Enum.TryParse<TravelTimingKind>(timing.Kind, ignoreCase: true, out var kind))
        {
            throw new ArgumentException("Unknown timing kind.", nameof(timing));
        }

        return kind switch
        {
            TravelTimingKind.Undecided => TravelTimingPreference.Undecided(),
            TravelTimingKind.ExactDates => TravelTimingPreference.Exact(
                ParseDate(timing.ExactStartDate, nameof(timing.ExactStartDate)),
                ParseDate(timing.ExactEndDate, nameof(timing.ExactEndDate))),
            TravelTimingKind.FlexibleRange => TravelTimingPreference.Flexible(
                ParseDate(timing.FlexibleEarliestStart, nameof(timing.FlexibleEarliestStart)),
                ParseDate(timing.FlexibleLatestStart, nameof(timing.FlexibleLatestStart)),
                timing.FlexibleMaxTripDurationDays),
            TravelTimingKind.ApproximatePeriod => TravelTimingPreference.Approximate(
                timing.ApproximateYear,
                timing.ApproximateMonth,
                timing.ApproximateSeason is null
                    ? null
                    : ParseEnum<TravelSeason>(timing.ApproximateSeason, nameof(timing.ApproximateSeason))),
            _ => throw new ArgumentOutOfRangeException(nameof(timing), kind, "Unsupported timing kind."),
        };
    }

    private static BudgetPreference? MapBudget(TripPlannerBudgetDraft budget)
    {
        ArgumentNullException.ThrowIfNull(budget);
        if (budget.MinimumAmount is null && budget.MaximumAmount is null)
        {
            return null;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(budget.CurrencyCode);
        return BudgetPreference.Create(
            CurrencyCode.Parse(budget.CurrencyCode),
            budget.MinimumAmount,
            budget.MaximumAmount);
    }

    private static IEnumerable<DestinationPreference> MapDestinations(TripPlannerDestinationDraft destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (destination.Undecided || destination.LogicalDestinationIds is null || destination.LogicalDestinationIds.Count == 0)
        {
            return [DestinationPreference.Undecided()];
        }

        return destination.LogicalDestinationIds
            .Select((id, index) => DestinationPreference.ForLogicalDestination(id, index));
    }

    private static LocalDate ParseDate(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Date is required.", paramName);
        }

        var parseResult = LocalDatePattern.Iso.Parse(value);
        if (!parseResult.Success)
        {
            throw new ArgumentException("Date must be yyyy-MM-dd.", paramName);
        }

        return parseResult.Value;
    }

    private static TEnum ParseEnum<TEnum>(string value, string paramName)
        where TEnum : struct, Enum
    {
        if (!Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed))
        {
            throw new ArgumentException($"Unknown {typeof(TEnum).Name} value.", paramName);
        }

        return parsed;
    }
}
