namespace TravelCore.Modules.Notification.Contracts;

/// <summary>
/// Controlled, server-side notification provider identifier. Not a delivery id and not a client-injected class name (P25-R3).
/// </summary>
public readonly record struct NotificationProviderKey
{
    public const int MinLength = 2;
    public const int MaxLength = 64;

    public string Value { get; }

    public NotificationProviderKey(string value)
    {
        if (!TryNormalize(value, out var normalized))
        {
            throw new ArgumentException(
                "NotificationProviderKey must be a controlled lowercase identifier (letter, digit, hyphen).",
                nameof(value));
        }

        Value = normalized;
    }

    public static bool TryParse(string? value, out NotificationProviderKey providerKey)
    {
        if (!TryNormalize(value, out var normalized))
        {
            providerKey = default;
            return false;
        }

        providerKey = new NotificationProviderKey(normalized);
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
