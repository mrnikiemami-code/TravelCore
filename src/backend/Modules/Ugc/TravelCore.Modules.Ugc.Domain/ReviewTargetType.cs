namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Controlled logical Review target kind (TC-P16-T003 / P16-R3). Not a peer module type.
/// </summary>
public sealed class ReviewTargetType : IEquatable<ReviewTargetType>
{
    public const string TourProductValue = "TourProduct";
    public const string PlaceValue = "Place";
    public const string AgencyValue = "Agency";
    public const int MaxLength = 64;

    private ReviewTargetType(string value) => Value = value;

    public static ReviewTargetType TourProduct { get; } = new(TourProductValue);

    public static ReviewTargetType Place { get; } = new(PlaceValue);

    public static ReviewTargetType Agency { get; } = new(AgencyValue);

    public static IReadOnlyCollection<string> AllowedValues { get; } =
        [TourProductValue, PlaceValue, AgencyValue];

    public string Value { get; }

    public static ReviewTargetType Parse(string? value)
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

        return trimmed switch
        {
            TourProductValue => TourProduct,
            PlaceValue => Place,
            AgencyValue => Agency,
            _ => throw new ArgumentException(
                $"Unknown Review TargetType '{trimmed}'. Allowed: {string.Join(", ", AllowedValues)}.",
                nameof(value)),
        };
    }

    public bool Equals(ReviewTargetType? other) =>
        other is not null && Value.Equals(other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is ReviewTargetType other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;

    public static bool operator ==(ReviewTargetType? left, ReviewTargetType? right) => Equals(left, right);

    public static bool operator !=(ReviewTargetType? left, ReviewTargetType? right) => !Equals(left, right);
}
