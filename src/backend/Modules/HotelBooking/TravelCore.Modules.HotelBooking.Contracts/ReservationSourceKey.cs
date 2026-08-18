namespace TravelCore.Modules.HotelBooking.Contracts;

/// <summary>
/// Controlled, server-side hotel reservation source identifier. Not user input.
/// Conceptually distinct from availability and rate source keys.
/// </summary>
public readonly record struct ReservationSourceKey
{
    public const int MinLength = 2;
    public const int MaxLength = 64;

    public string Value { get; }

    public ReservationSourceKey(string value)
    {
        if (!TryNormalize(value, out var normalized))
        {
            throw new ArgumentException(
                "ReservationSourceKey must be a controlled lowercase identifier (letter, digit, hyphen).",
                nameof(value));
        }

        Value = normalized;
    }

    public static bool TryParse(string? value, out ReservationSourceKey sourceKey)
    {
        if (!TryNormalize(value, out var normalized))
        {
            sourceKey = default;
            return false;
        }

        sourceKey = new ReservationSourceKey(normalized);
        return true;
    }

    public override string ToString() => Value;

    private static bool TryNormalize(string? value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim().ToLowerInvariant();
        if (trimmed.Length is < MinLength or > MaxLength)
        {
            return false;
        }

        if (trimmed[0] is < 'a' or > 'z')
        {
            return false;
        }

        foreach (var ch in trimmed)
        {
            if (ch is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '-')
            {
                continue;
            }

            return false;
        }

        normalized = trimmed;
        return true;
    }
}
