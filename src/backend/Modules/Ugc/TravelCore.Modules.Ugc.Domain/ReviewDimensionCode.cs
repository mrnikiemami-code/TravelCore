using System.Text.RegularExpressions;

namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Normalized dimension code for structured child ratings. Not a hardcoded hotel/food/guide column.
/// </summary>
public readonly record struct ReviewDimensionCode
{
    public const int MaxLength = 64;
    private static readonly Regex Pattern = new("^[a-z][a-z0-9_]{0,63}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    private ReviewDimensionCode(string value) => Value = value;

    public static ReviewDimensionCode Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > MaxLength || !Pattern.IsMatch(normalized))
        {
            throw new ArgumentException(
                "DimensionCode must be lowercase [a-z][a-z0-9_]{0,63}.",
                nameof(value));
        }

        return new ReviewDimensionCode(normalized);
    }

    public override string ToString() => Value;
}
