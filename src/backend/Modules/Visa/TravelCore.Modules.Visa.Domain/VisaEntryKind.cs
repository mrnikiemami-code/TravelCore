namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Controlled entry policy (TC-P17-T005 / P17-R5). Not inferred from processing, validity, or stay.
/// </summary>
public sealed class VisaEntryKind : IEquatable<VisaEntryKind>
{
    public const string SingleValue = "Single";
    public const string DoubleValue = "Double";
    public const string MultipleValue = "Multiple";
    public const int MaxLength = 16;

    private VisaEntryKind(string value) => Value = value;

    public static VisaEntryKind Single { get; } = new(SingleValue);

    public static VisaEntryKind Double { get; } = new(DoubleValue);

    public static VisaEntryKind Multiple { get; } = new(MultipleValue);

    public static IReadOnlyCollection<string> AllowedValues { get; } =
        [SingleValue, DoubleValue, MultipleValue];

    public string Value { get; }

    public static VisaEntryKind Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Entry kind is required.", nameof(value));
        }

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Entry kind max length is {MaxLength}.", nameof(value));
        }

        return trimmed switch
        {
            SingleValue => Single,
            DoubleValue => Double,
            MultipleValue => Multiple,
            _ => throw new ArgumentException(
                $"Unknown Visa entry kind '{trimmed}'. Allowed: {string.Join(", ", AllowedValues)}.",
                nameof(value)),
        };
    }

    public bool Equals(VisaEntryKind? other) =>
        other is not null && Value.Equals(other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is VisaEntryKind other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;

    public static bool operator ==(VisaEntryKind? left, VisaEntryKind? right) => Equals(left, right);

    public static bool operator !=(VisaEntryKind? left, VisaEntryKind? right) => !Equals(left, right);
}
