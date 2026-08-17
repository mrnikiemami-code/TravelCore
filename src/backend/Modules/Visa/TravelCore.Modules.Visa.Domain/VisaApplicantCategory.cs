namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Controlled applicant-category value (TC-P17-T003 / P17-R3). Not an age/eligibility engine.
/// </summary>
public sealed class VisaApplicantCategory : IEquatable<VisaApplicantCategory>
{
    public const string AdultValue = "Adult";
    public const string MinorValue = "Minor";
    public const string OtherValue = "Other";
    public const int MaxLength = 32;

    private VisaApplicantCategory(string value) => Value = value;

    public static VisaApplicantCategory Adult { get; } = new(AdultValue);

    public static VisaApplicantCategory Minor { get; } = new(MinorValue);

    public static VisaApplicantCategory Other { get; } = new(OtherValue);

    public static IReadOnlyCollection<string> AllowedValues { get; } =
        [AdultValue, MinorValue, OtherValue];

    public string Value { get; }

    public static VisaApplicantCategory Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("ApplicantCategory is required when provided.", nameof(value));
        }

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"ApplicantCategory max length is {MaxLength}.", nameof(value));
        }

        return trimmed switch
        {
            AdultValue => Adult,
            MinorValue => Minor,
            OtherValue => Other,
            _ => throw new ArgumentException(
                $"Unknown Visa ApplicantCategory '{trimmed}'. Allowed: {string.Join(", ", AllowedValues)}.",
                nameof(value)),
        };
    }

    public static VisaApplicantCategory? ParseOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : Parse(value);

    public bool Equals(VisaApplicantCategory? other) =>
        other is not null && Value.Equals(other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is VisaApplicantCategory other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;

    public static bool operator ==(VisaApplicantCategory? left, VisaApplicantCategory? right) =>
        Equals(left, right);

    public static bool operator !=(VisaApplicantCategory? left, VisaApplicantCategory? right) =>
        !Equals(left, right);
}
