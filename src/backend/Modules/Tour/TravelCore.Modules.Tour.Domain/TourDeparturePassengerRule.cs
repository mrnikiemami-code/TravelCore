namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Passenger acceptance rules for a TourDeparture (P11-R7 · TC-P11-T007).
/// Descriptive occupancy policy only — not Passenger entity, Traveller profile, or Booking.
/// </summary>
public sealed class TourDeparturePassengerRule
{
    private TourDeparturePassengerRule()
    {
    }

    private TourDeparturePassengerRule(
        int minimumAdults,
        bool childAllowed,
        bool infantAllowed,
        int maximumPassengers)
    {
        MinimumAdults = minimumAdults;
        ChildAllowed = childAllowed;
        InfantAllowed = infantAllowed;
        MaximumPassengers = maximumPassengers;
    }

    public int MinimumAdults { get; private set; }

    public bool ChildAllowed { get; private set; }

    public bool InfantAllowed { get; private set; }

    public int MaximumPassengers { get; private set; }

    public static TourDeparturePassengerRule Create(
        int minimumAdults,
        bool childAllowed,
        bool infantAllowed,
        int maximumPassengers)
    {
        if (minimumAdults < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimumAdults),
                minimumAdults,
                "MinimumAdults must be >= 0.");
        }

        if (maximumPassengers <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumPassengers),
                maximumPassengers,
                "MaximumPassengers must be > 0.");
        }

        if (maximumPassengers < minimumAdults)
        {
            throw new ArgumentException(
                "MaximumPassengers must be greater than or equal to MinimumAdults.",
                nameof(maximumPassengers));
        }

        return new TourDeparturePassengerRule(
            minimumAdults,
            childAllowed,
            infantAllowed,
            maximumPassengers);
    }
}
