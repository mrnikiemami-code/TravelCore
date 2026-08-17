namespace TravelCore.Modules.Pricing.Domain;

/// <summary>
/// Polymorphic logical target for a buyable <see cref="Price"/> (P12-R3).
/// Pricing stays generic — does not reference Tour module types.
/// Initial allowed value: <see cref="TourDepartureValue"/> only.
/// </summary>
public sealed class PriceTargetType : IEquatable<PriceTargetType>
{
    /// <summary>Initial buyable/executable Price target (logical Guid only — no FK).</summary>
    public const string TourDepartureValue = "TourDeparture";

    public const int MaxLength = 64;

    private PriceTargetType(string value) => Value = value;

    public static PriceTargetType TourDeparture { get; } = new(TourDepartureValue);

    public string Value { get; }

    /// <summary>
    /// Parses a required target type. Unknown values are rejected (only TourDeparture for now).
    /// </summary>
    public static PriceTargetType Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("TargetType is required.", nameof(value));
        }

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"TargetType max length is {MaxLength}.", nameof(value));
        }

        if (!trimmed.Equals(TourDepartureValue, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Unknown Price TargetType '{trimmed}'. Allowed: {TourDepartureValue}.",
                nameof(value));
        }

        return TourDeparture;
    }

    public bool Equals(PriceTargetType? other) =>
        other is not null && Value.Equals(other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is PriceTargetType other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;

    public static bool operator ==(PriceTargetType? left, PriceTargetType? right) => Equals(left, right);

    public static bool operator !=(PriceTargetType? left, PriceTargetType? right) => !Equals(left, right);
}
