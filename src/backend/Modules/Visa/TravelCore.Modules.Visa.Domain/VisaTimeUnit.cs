namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Controlled time-quantity unit for processing, validity, or stay facts (TC-P17-T005 / P17-R5).
/// Not a single Duration field and not a regulatory calendar engine.
/// </summary>
public sealed class VisaTimeUnit : IEquatable<VisaTimeUnit>
{
    public const string DaysValue = "Days";
    public const string BusinessDaysValue = "BusinessDays";
    public const string MonthsValue = "Months";
    public const string YearsValue = "Years";
    public const int MaxLength = 16;

    private VisaTimeUnit(string value) => Value = value;

    public static VisaTimeUnit Days { get; } = new(DaysValue);

    public static VisaTimeUnit BusinessDays { get; } = new(BusinessDaysValue);

    public static VisaTimeUnit Months { get; } = new(MonthsValue);

    public static VisaTimeUnit Years { get; } = new(YearsValue);

    public static IReadOnlyCollection<string> AllowedValues { get; } =
        [DaysValue, BusinessDaysValue, MonthsValue, YearsValue];

    public string Value { get; }

    public static VisaTimeUnit Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Time unit is required.", nameof(value));
        }

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Time unit max length is {MaxLength}.", nameof(value));
        }

        return trimmed switch
        {
            DaysValue => Days,
            BusinessDaysValue => BusinessDays,
            MonthsValue => Months,
            YearsValue => Years,
            _ => throw new ArgumentException(
                $"Unknown Visa time unit '{trimmed}'. Allowed: {string.Join(", ", AllowedValues)}.",
                nameof(value)),
        };
    }

    public bool Equals(VisaTimeUnit? other) =>
        other is not null && Value.Equals(other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is VisaTimeUnit other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;

    public static bool operator ==(VisaTimeUnit? left, VisaTimeUnit? right) => Equals(left, right);

    public static bool operator !=(VisaTimeUnit? left, VisaTimeUnit? right) => !Equals(left, right);
}
