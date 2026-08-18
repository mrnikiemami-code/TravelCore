namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Controlled, server-side provider identifier. Not a PaymentId and not a client-injected class name (P20-R3).
/// </summary>
public readonly record struct ProviderKey
{
    public const int MinLength = 2;
    public const int MaxLength = 64;

    public string Value { get; }

    public ProviderKey(string value)
    {
        if (!TryNormalize(value, out var normalized))
        {
            throw new ArgumentException(
                "ProviderKey must be a controlled lowercase identifier (letter, digit, hyphen).",
                nameof(value));
        }

        Value = normalized;
    }

    public static bool TryParse(string? value, out ProviderKey providerKey)
    {
        if (!TryNormalize(value, out var normalized))
        {
            providerKey = default;
            return false;
        }

        providerKey = new ProviderKey(normalized);
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
