using NodaTime;

namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Controlled travel timing preference (P18-R4). Not a schedule engine.
/// </summary>
public sealed class TravelTimingPreference
{
    private TravelTimingPreference()
    {
    }

    private TravelTimingPreference(TravelTimingKind kind)
    {
        Kind = kind;
    }

    public TravelTimingKind Kind { get; private set; }

    public LocalDate? ExactStartDate { get; private set; }

    public LocalDate? ExactEndDate { get; private set; }

    public LocalDate? FlexibleEarliestStart { get; private set; }

    public LocalDate? FlexibleLatestStart { get; private set; }

    public int? FlexibleMaxTripDurationDays { get; private set; }

    public int? ApproximateYear { get; private set; }

    public int? ApproximateMonth { get; private set; }

    public TravelSeason? ApproximateSeason { get; private set; }

    public static TravelTimingPreference Undecided() => new(TravelTimingKind.Undecided);

    public static TravelTimingPreference Exact(LocalDate startDate, LocalDate endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("EndDate must be on or after StartDate.", nameof(endDate));
        }

        return new TravelTimingPreference(TravelTimingKind.ExactDates)
        {
            ExactStartDate = startDate,
            ExactEndDate = endDate,
        };
    }

    public static TravelTimingPreference Flexible(
        LocalDate earliestStart,
        LocalDate latestStart,
        int? maxTripDurationDays = null)
    {
        if (latestStart < earliestStart)
        {
            throw new ArgumentException("LatestStart must be on or after EarliestStart.", nameof(latestStart));
        }

        if (maxTripDurationDays is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxTripDurationDays));
        }

        return new TravelTimingPreference(TravelTimingKind.FlexibleRange)
        {
            FlexibleEarliestStart = earliestStart,
            FlexibleLatestStart = latestStart,
            FlexibleMaxTripDurationDays = maxTripDurationDays,
        };
    }

    public static TravelTimingPreference Approximate(
        int? year = null,
        int? month = null,
        TravelSeason? season = null)
    {
        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be 1..12 when provided.");
        }

        if (year is null && month is null && season is null)
        {
            throw new ArgumentException("Approximate period requires year, month, or season.");
        }

        return new TravelTimingPreference(TravelTimingKind.ApproximatePeriod)
        {
            ApproximateYear = year,
            ApproximateMonth = month,
            ApproximateSeason = season,
        };
    }

    internal TravelTimingPreference CaptureCopy() => new(Kind)
    {
        ExactStartDate = ExactStartDate,
        ExactEndDate = ExactEndDate,
        FlexibleEarliestStart = FlexibleEarliestStart,
        FlexibleLatestStart = FlexibleLatestStart,
        FlexibleMaxTripDurationDays = FlexibleMaxTripDurationDays,
        ApproximateYear = ApproximateYear,
        ApproximateMonth = ApproximateMonth,
        ApproximateSeason = ApproximateSeason,
    };
}
