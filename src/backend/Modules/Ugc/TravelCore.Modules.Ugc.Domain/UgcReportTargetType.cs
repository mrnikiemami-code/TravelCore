namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Controlled UgcReport target kind (TC-P16-T007 / P16-R7). Logical reference only — no peer FK.
/// </summary>
public sealed class UgcReportTargetType : IEquatable<UgcReportTargetType>
{
    public const string ReviewValue = "Review";
    public const string TravelogueValue = "Travelogue";
    public const string UserPhotoValue = "UserPhoto";
    public const string CommentValue = "Comment";
    public const int MaxLength = 64;

    private UgcReportTargetType(string value) => Value = value;

    public static UgcReportTargetType Review { get; } = new(ReviewValue);

    public static UgcReportTargetType Travelogue { get; } = new(TravelogueValue);

    public static UgcReportTargetType UserPhoto { get; } = new(UserPhotoValue);

    public static UgcReportTargetType Comment { get; } = new(CommentValue);

    public static IReadOnlyCollection<string> AllowedValues { get; } =
        [ReviewValue, TravelogueValue, UserPhotoValue, CommentValue];

    public string Value { get; }

    public static UgcReportTargetType Parse(string? value)
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
            UserPhotoValue => UserPhoto,
            CommentValue => Comment,
            _ => throw new ArgumentException(
                $"Unknown UgcReport TargetType '{trimmed}'. Allowed: {string.Join(", ", AllowedValues)}.",
                nameof(value)),
        };
    }

    public bool Equals(UgcReportTargetType? other) =>
        other is not null && Value.Equals(other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is UgcReportTargetType other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;

    public static bool operator ==(UgcReportTargetType? left, UgcReportTargetType? right) => Equals(left, right);

    public static bool operator !=(UgcReportTargetType? left, UgcReportTargetType? right) => !Equals(left, right);
}
