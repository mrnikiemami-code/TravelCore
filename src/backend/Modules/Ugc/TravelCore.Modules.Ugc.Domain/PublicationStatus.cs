namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Public-visibility status (TC-P16-T007 / P16-R7). Distinct from <see cref="ModerationStatus"/>.
/// Published != SEO Indexed.
/// </summary>
public sealed class PublicationStatus : IEquatable<PublicationStatus>
{
    public const string DraftValue = "Draft";
    public const string PublishedValue = "Published";
    public const string HiddenValue = "Hidden";
    public const string ArchivedValue = "Archived";
    public const int MaxLength = 32;

    private PublicationStatus(string value) => Value = value;

    public static PublicationStatus Draft { get; } = new(DraftValue);

    public static PublicationStatus Published { get; } = new(PublishedValue);

    public static PublicationStatus Hidden { get; } = new(HiddenValue);

    public static PublicationStatus Archived { get; } = new(ArchivedValue);

    public static IReadOnlyCollection<string> AllowedValues { get; } =
        [DraftValue, PublishedValue, HiddenValue, ArchivedValue];

    public string Value { get; }

    public static PublicationStatus Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("PublicationStatus is required.", nameof(value));
        }

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"PublicationStatus max length is {MaxLength}.", nameof(value));
        }

        return trimmed switch
        {
            DraftValue => Draft,
            PublishedValue => Published,
            HiddenValue => Hidden,
            ArchivedValue => Archived,
            _ => throw new ArgumentException(
                $"Unknown PublicationStatus '{trimmed}'. Allowed: {string.Join(", ", AllowedValues)}.",
                nameof(value)),
        };
    }

    public bool Equals(PublicationStatus? other) =>
        other is not null && Value.Equals(other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is PublicationStatus other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;

    public static bool operator ==(PublicationStatus? left, PublicationStatus? right) => Equals(left, right);

    public static bool operator !=(PublicationStatus? left, PublicationStatus? right) => !Equals(left, right);
}
