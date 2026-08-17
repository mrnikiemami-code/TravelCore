namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Departure execution capacity rules (P11-R3 · TC-P11-T003).
/// Describes planned pax bounds only — Booking owns reservation consumption later.
/// </summary>
public sealed class TourDepartureCapacity
{
    private TourDepartureCapacity()
    {
    }

    private TourDepartureCapacity(int minimumPax, int maximumPax)
    {
        MinimumPax = minimumPax;
        MaximumPax = maximumPax;
    }

    public int MinimumPax { get; private set; }

    public int MaximumPax { get; private set; }

    public static TourDepartureCapacity Create(int minimumPax, int maximumPax)
    {
        if (minimumPax < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumPax), minimumPax, "MinimumPax must be >= 0.");
        }

        if (maximumPax <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumPax), maximumPax, "MaximumPax must be > 0.");
        }

        if (maximumPax < minimumPax)
        {
            throw new ArgumentException("MaximumPax must be greater than or equal to MinimumPax.", nameof(maximumPax));
        }

        return new TourDepartureCapacity(minimumPax, maximumPax);
    }
}
