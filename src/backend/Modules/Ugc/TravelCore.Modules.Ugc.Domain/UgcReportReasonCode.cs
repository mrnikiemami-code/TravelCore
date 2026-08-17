namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Controlled extensible report reason (TC-P16-T007 / P16-R7). Not a generic moderation/rules engine.
/// </summary>
public sealed class UgcReportReasonCode : IEquatable<UgcReportReasonCode>
{
    public const string SpamValue = "spam";
    public const string AbuseValue = "abuse";
    public const string OffTopicValue = "off_topic";
    public const string CopyrightValue = "copyright";
    public const string OtherValue = "other";
    public const int MaxLength = 64;

    private UgcReportReasonCode(string value) => Value = value;

    public static UgcReportReasonCode Spam { get; } = new(SpamValue);

    public static UgcReportReasonCode Abuse { get; } = new(AbuseValue);

    public static UgcReportReasonCode OffTopic { get; } = new(OffTopicValue);

    public static UgcReportReasonCode Copyright { get; } = new(CopyrightValue);

    public static UgcReportReasonCode Other { get; } = new(OtherValue);

    public static IReadOnlyCollection<string> AllowedValues { get; } =
        [SpamValue, AbuseValue, OffTopicValue, CopyrightValue, OtherValue];

    public string Value { get; }

    public static UgcReportReasonCode Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("ReasonCode is required.", nameof(value));
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > MaxLength)
        {
            throw new ArgumentException($"ReasonCode max length is {MaxLength}.", nameof(value));
        }

        return normalized switch
        {
            SpamValue => Spam,
            AbuseValue => Abuse,
            OffTopicValue => OffTopic,
            CopyrightValue => Copyright,
            OtherValue => Other,
            _ => throw new ArgumentException(
                $"Unknown ReasonCode '{normalized}'. Allowed: {string.Join(", ", AllowedValues)}.",
                nameof(value)),
        };
    }

    public bool Equals(UgcReportReasonCode? other) =>
        other is not null && Value.Equals(other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is UgcReportReasonCode other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;

    public static bool operator ==(UgcReportReasonCode? left, UgcReportReasonCode? right) => Equals(left, right);

    public static bool operator !=(UgcReportReasonCode? left, UgcReportReasonCode? right) => !Equals(left, right);
}
