using NodaTime;

namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Departure travel-date schedule (P11-R2 · TC-P11-T002). Local travel dates + required IANA timezone.
/// Exact moments (Instant) are not stored here — use LocalDate for travel dates only.
/// </summary>
public sealed class TourDepartureSchedule
{
    public const int TimeZoneIdMaxLength = 64;

    private TourDepartureSchedule()
    {
        TimeZoneId = null!;
    }

    private TourDepartureSchedule(LocalDate startDate, LocalDate endDate, string timeZoneId)
    {
        StartDate = startDate;
        EndDate = endDate;
        TimeZoneId = timeZoneId;
    }

    public LocalDate StartDate { get; private set; }

    public LocalDate EndDate { get; private set; }

    /// <summary>IANA TZDB id (e.g. Asia/Tehran). Required.</summary>
    public string TimeZoneId { get; private set; }

    public static TourDepartureSchedule Create(LocalDate startDate, LocalDate endDate, string timeZoneId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);
        var trimmed = timeZoneId.Trim();
        if (trimmed.Length > TimeZoneIdMaxLength)
        {
            throw new ArgumentException(
                $"TimeZoneId max length is {TimeZoneIdMaxLength}.",
                nameof(timeZoneId));
        }

        var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(trimmed)
            ?? throw new ArgumentException($"Unknown IANA time zone id '{trimmed}'.", nameof(timeZoneId));

        if (endDate < startDate)
        {
            throw new ArgumentException("EndDate must be greater than or equal to StartDate.", nameof(endDate));
        }

        return new TourDepartureSchedule(startDate, endDate, zone.Id);
    }
}
