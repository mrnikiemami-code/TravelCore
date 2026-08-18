namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Child age at check-in. Technical range only (0–120). Not a hotel pricing child/adult boundary.
/// </summary>
public readonly record struct HotelGuestAgeAtCheckIn
{
    public const int MinYears = 0;
    public const int MaxYears = 120;

    public int Years { get; }

    public HotelGuestAgeAtCheckIn(int years)
    {
        if (years < MinYears || years > MaxYears)
        {
            throw new ArgumentOutOfRangeException(
                nameof(years),
                years,
                $"AgeAtCheckIn must be between {MinYears} and {MaxYears}.");
        }

        Years = years;
    }
}
