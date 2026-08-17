namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Controlled requirement level (TC-P17-T004 / P17-R4). Conditional is a fact label, not an executable engine.
/// </summary>
public sealed class VisaRequirementLevel : IEquatable<VisaRequirementLevel>
{
    public const string RequiredValue = "Required";
    public const string ConditionalValue = "Conditional";
    public const string OptionalValue = "Optional";
    public const int MaxLength = 16;

    private VisaRequirementLevel(string value) => Value = value;

    public static VisaRequirementLevel Required { get; } = new(RequiredValue);

    public static VisaRequirementLevel Conditional { get; } = new(ConditionalValue);

    public static VisaRequirementLevel Optional { get; } = new(OptionalValue);

    public static IReadOnlyCollection<string> AllowedValues { get; } =
        [RequiredValue, ConditionalValue, OptionalValue];

    public string Value { get; }

    public static VisaRequirementLevel Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("RequirementLevel is required.", nameof(value));
        }

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"RequirementLevel max length is {MaxLength}.", nameof(value));
        }

        return trimmed switch
        {
            RequiredValue => Required,
            ConditionalValue => Conditional,
            OptionalValue => Optional,
            _ => throw new ArgumentException(
                $"Unknown Visa RequirementLevel '{trimmed}'. Allowed: {string.Join(", ", AllowedValues)}.",
                nameof(value)),
        };
    }

    public bool Equals(VisaRequirementLevel? other) =>
        other is not null && Value.Equals(other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is VisaRequirementLevel other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;

    public static bool operator ==(VisaRequirementLevel? left, VisaRequirementLevel? right) =>
        Equals(left, right);

    public static bool operator !=(VisaRequirementLevel? left, VisaRequirementLevel? right) =>
        !Equals(left, right);
}
