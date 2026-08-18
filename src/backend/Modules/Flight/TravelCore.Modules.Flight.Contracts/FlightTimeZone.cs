using NodaTime;

namespace TravelCore.Modules.Flight.Contracts;

public static class FlightTimeZone
{
    public const int IdMaxLength = 64;

    public static string RequireIanaId(string timeZoneId, string paramName)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw new ArgumentException("IANA timezone identifier is required.", paramName);
        }

        var trimmed = timeZoneId.Trim();
        if (trimmed.Length > IdMaxLength)
        {
            throw new ArgumentException($"IANA timezone identifier max length is {IdMaxLength}.", paramName);
        }

        if (DateTimeZoneProviders.Tzdb.GetZoneOrNull(trimmed) is null)
        {
            throw new ArgumentException($"Unknown IANA timezone identifier '{trimmed}'.", paramName);
        }

        return trimmed;
    }
}
