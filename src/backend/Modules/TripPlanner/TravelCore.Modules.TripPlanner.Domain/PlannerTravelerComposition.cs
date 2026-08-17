namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Structured traveler counts for planning (P18-R4). Not BookingPassenger identity.
/// </summary>
public sealed class PlannerTravelerComposition
{
    private PlannerTravelerComposition()
    {
    }

    private PlannerTravelerComposition(int adultCount, int childCount, int infantCount)
    {
        AdultCount = adultCount;
        ChildCount = childCount;
        InfantCount = infantCount;
    }

    public int AdultCount { get; private set; }

    public int ChildCount { get; private set; }

    public int InfantCount { get; private set; }

    public int TotalCount => AdultCount + ChildCount + InfantCount;

    public static PlannerTravelerComposition Create(int adultCount, int childCount = 0, int infantCount = 0)
    {
        if (adultCount < 0 || childCount < 0 || infantCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(adultCount), "Traveler counts must be non-negative.");
        }

        return new PlannerTravelerComposition(adultCount, childCount, infantCount);
    }

    internal void ValidateForLeadSubmission()
    {
        if (TotalCount <= 0)
        {
            throw new InvalidOperationException("Submitted lead requires at least one traveler when composition is provided.");
        }
    }

    internal PlannerTravelerComposition CaptureCopy() => new(AdultCount, ChildCount, InfantCount);
}
