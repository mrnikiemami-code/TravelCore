using System.Diagnostics.CodeAnalysis;

namespace TravelCore.Money;

/// <summary>
/// Explicit currency identity (ADR 0003). Not a closed catalog — codes are extensible.
/// </summary>
public sealed class CurrencyCode : IEquatable<CurrencyCode>
{
    public const int MinLength = 3;
    public const int MaxLength = 12;

    private CurrencyCode(string value) => Value = value;

    /// <summary>Canonical uppercase currency code (invariant culture).</summary>
    public string Value { get; }

    /// <summary>
    /// Parses and canonicalizes a currency code: trim, invariant uppercase, length 3–12, ASCII A–Z only.
    /// </summary>
    public static CurrencyCode Parse(string code)
    {
        if (!TryParse(code, out var parsed))
        {
            throw new ArgumentException("Currency code must be 3–12 ASCII letters A–Z after trim/uppercase normalization.", nameof(code));
        }

        return parsed;
    }

    public static bool TryParse([NotNullWhen(true)] string? code, [NotNullWhen(true)] out CurrencyCode? result)
    {
        result = null;
        if (code is null)
        {
            return false;
        }

        var trimmed = code.AsSpan().Trim();
        if (trimmed.Length is < MinLength or > MaxLength)
        {
            return false;
        }

        Span<char> buffer = stackalloc char[MaxLength];
        for (var i = 0; i < trimmed.Length; i++)
        {
            var c = trimmed[i];
            if (c is >= 'a' and <= 'z')
            {
                buffer[i] = (char)(c - 32);
            }
            else if (c is >= 'A' and <= 'Z')
            {
                buffer[i] = c;
            }
            else
            {
                return false;
            }
        }

        result = new CurrencyCode(new string(buffer[..trimmed.Length]));
        return true;
    }

    public bool Equals(CurrencyCode? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is CurrencyCode other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public override string ToString() => Value;

    public static bool operator ==(CurrencyCode? left, CurrencyCode? right) => Equals(left, right);

    public static bool operator !=(CurrencyCode? left, CurrencyCode? right) => !Equals(left, right);
}
