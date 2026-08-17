namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Report handling status (TC-P16-T007 / P16-R7). Report is moderation input only.
/// </summary>
public sealed class UgcReportStatus : IEquatable<UgcReportStatus>
{
    public const string OpenValue = "Open";
    public const string ResolvedValue = "Resolved";
    public const string DismissedValue = "Dismissed";
    public const int MaxLength = 32;

    private UgcReportStatus(string value) => Value = value;

    public static UgcReportStatus Open { get; } = new(OpenValue);

    public static UgcReportStatus Resolved { get; } = new(ResolvedValue);

    public static UgcReportStatus Dismissed { get; } = new(DismissedValue);

    public static IReadOnlyCollection<string> AllowedValues { get; } =
        [OpenValue, ResolvedValue, DismissedValue];

    public string Value { get; }

    public static UgcReportStatus Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("ReportStatus is required.", nameof(value));
        }

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"ReportStatus max length is {MaxLength}.", nameof(value));
        }

        return trimmed switch
        {
            OpenValue => Open,
            ResolvedValue => Resolved,
            DismissedValue => Dismissed,
            _ => throw new ArgumentException(
                $"Unknown ReportStatus '{trimmed}'. Allowed: {string.Join(", ", AllowedValues)}.",
                nameof(value)),
        };
    }

    public bool Equals(UgcReportStatus? other) =>
        other is not null && Value.Equals(other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is UgcReportStatus other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;

    public static bool operator ==(UgcReportStatus? left, UgcReportStatus? right) => Equals(left, right);

    public static bool operator !=(UgcReportStatus? left, UgcReportStatus? right) => !Equals(left, right);
}
