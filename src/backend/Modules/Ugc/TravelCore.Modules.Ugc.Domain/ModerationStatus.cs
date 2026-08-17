namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Content-control status (TC-P16-T007 / P16-R7). Distinct from <see cref="PublicationStatus"/>.
/// Approved != Published.
/// </summary>
public sealed class ModerationStatus : IEquatable<ModerationStatus>
{
    public const string PendingValue = "Pending";
    public const string ApprovedValue = "Approved";
    public const string RejectedValue = "Rejected";
    public const int MaxLength = 32;

    private ModerationStatus(string value) => Value = value;

    public static ModerationStatus Pending { get; } = new(PendingValue);

    public static ModerationStatus Approved { get; } = new(ApprovedValue);

    public static ModerationStatus Rejected { get; } = new(RejectedValue);

    public static IReadOnlyCollection<string> AllowedValues { get; } =
        [PendingValue, ApprovedValue, RejectedValue];

    public string Value { get; }

    public static ModerationStatus Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("ModerationStatus is required.", nameof(value));
        }

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"ModerationStatus max length is {MaxLength}.", nameof(value));
        }

        return trimmed switch
        {
            PendingValue => Pending,
            ApprovedValue => Approved,
            RejectedValue => Rejected,
            _ => throw new ArgumentException(
                $"Unknown ModerationStatus '{trimmed}'. Allowed: {string.Join(", ", AllowedValues)}.",
                nameof(value)),
        };
    }

    public bool Equals(ModerationStatus? other) =>
        other is not null && Value.Equals(other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is ModerationStatus other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;

    public static bool operator ==(ModerationStatus? left, ModerationStatus? right) => Equals(left, right);

    public static bool operator !=(ModerationStatus? left, ModerationStatus? right) => !Equals(left, right);
}
