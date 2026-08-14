using NodaTime;

namespace TravelCore.Time;

/// <summary>
/// Canonical TravelCore temporal conventions (ADR 0004). Types themselves come from NodaTime.
/// </summary>
public static class TravelCoreTemporal
{
    /// <summary>
    /// Canonical IANA / TZDB timezone provider for TravelCore.
    /// </summary>
    public static IDateTimeZoneProvider TimeZones { get; } = DateTimeZoneProviders.Tzdb;
}
