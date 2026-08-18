using TravelCore.Modules.Booking.Contracts;

namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Booking-owned per-TourDeparture consumption/concurrency boundary (P19-R3).
/// Not Tour capacity-definition authority. Not a cloned TourDeparture.
/// Effective consumption = Active + Consumed. Released/Expired do not count.
/// </summary>
public sealed class DepartureCapacityAccount
{
    private DepartureCapacityAccount()
    {
    }

    private DepartureCapacityAccount(TourDepartureReference tourDeparture)
    {
        TourDeparture = tourDeparture;
        ActiveSeats = 0;
        ConsumedSeats = 0;
        Version = 0;
    }

    public TourDepartureReference TourDeparture { get; private set; }

    public int ActiveSeats { get; private set; }

    public int ConsumedSeats { get; private set; }

    public int Version { get; private set; }

    public int EffectiveSeats => ActiveSeats + ConsumedSeats;

    public static DepartureCapacityAccount Create(TourDepartureReference tourDeparture)
    {
        if (tourDeparture.LogicalId == Guid.Empty)
        {
            throw new ArgumentException("TourDeparture reference cannot be empty.", nameof(tourDeparture));
        }

        return new DepartureCapacityAccount(tourDeparture);
    }

    public int Available(int configuredCapacity)
    {
        EnsureConfigured(configuredCapacity);
        return Math.Max(0, configuredCapacity - EffectiveSeats);
    }

    public void Reserve(int seatCount, int configuredCapacity)
    {
        EnsurePositiveSeats(seatCount);
        EnsureConfigured(configuredCapacity);
        var available = Available(configuredCapacity);
        if (seatCount > available)
        {
            throw new InsufficientCapacityException(seatCount, available);
        }

        ActiveSeats += seatCount;
        Version++;
    }

    public void ReleaseActive(int seatCount)
    {
        EnsurePositiveSeats(seatCount);
        if (seatCount > ActiveSeats)
        {
            throw new InvalidOperationException("Cannot release more seats than currently Active.");
        }

        ActiveSeats -= seatCount;
        Version++;
    }

    public void ConsumeActive(int seatCount)
    {
        EnsurePositiveSeats(seatCount);
        if (seatCount > ActiveSeats)
        {
            throw new InvalidOperationException("Cannot consume more seats than currently Active.");
        }

        ActiveSeats -= seatCount;
        ConsumedSeats += seatCount;
        Version++;
    }

    private static void EnsurePositiveSeats(int seatCount)
    {
        if (seatCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seatCount), seatCount, "SeatCount must be > 0.");
        }
    }

    private static void EnsureConfigured(int configuredCapacity)
    {
        if (configuredCapacity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(configuredCapacity),
                configuredCapacity,
                "ConfiguredCapacity must be > 0.");
        }
    }
}
