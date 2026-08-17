namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Controlled logical Comment target kind (TC-P16-T006 / P16-R6). UGC-owned content only.
/// </summary>
public sealed class CommentTargetType : IEquatable<CommentTargetType>
{
    public const string ReviewValue = "Review";
    public const string TravelogueValue = "Travelogue";
    public const int MaxLength = 64;

    private CommentTargetType(string value) => Value = value;

    public static CommentTargetType Review { get; } = new(ReviewValue);

    public static CommentTargetType Travelogue { get; } = new(TravelogueValue);

    public static IReadOnlyCollection<string> AllowedValues { get; } =
        [ReviewValue, TravelogueValue];

    public string Value { get; }

    public static CommentTargetType Parse(string? value)
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
            ReviewValue => Review,
            TravelogueValue => Travelogue,
            _ => throw new ArgumentException(
                $"Unknown Comment TargetType '{trimmed}'. Allowed: {string.Join(", ", AllowedValues)}.",
                nameof(value)),
        };
    }

    public bool Equals(CommentTargetType? other) =>
        other is not null && Value.Equals(other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is CommentTargetType other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;

    public static bool operator ==(CommentTargetType? left, CommentTargetType? right) => Equals(left, right);

    public static bool operator !=(CommentTargetType? left, CommentTargetType? right) => !Equals(left, right);
}
