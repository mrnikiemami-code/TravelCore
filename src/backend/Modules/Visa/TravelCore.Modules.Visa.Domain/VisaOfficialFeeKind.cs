namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Controlled official/regulatory fee kind (TC-P17-T006 / P17-R6).
/// Not markup, commission, discount, or a commercial service charge.
/// </summary>
public sealed class VisaOfficialFeeKind : IEquatable<VisaOfficialFeeKind>
{
    public const string ApplicationValue = "Application";
    public const string IssuanceValue = "Issuance";
    public const string EmbassyValue = "Embassy";
    public const string ServiceCenterValue = "ServiceCenter";
    public const int MaxLength = 32;

    private VisaOfficialFeeKind(string value) => Value = value;

    public static VisaOfficialFeeKind Application { get; } = new(ApplicationValue);

    public static VisaOfficialFeeKind Issuance { get; } = new(IssuanceValue);

    public static VisaOfficialFeeKind Embassy { get; } = new(EmbassyValue);

    public static VisaOfficialFeeKind ServiceCenter { get; } = new(ServiceCenterValue);

    public static IReadOnlyCollection<string> AllowedValues { get; } =
        [ApplicationValue, IssuanceValue, EmbassyValue, ServiceCenterValue];

    public string Value { get; }

    public static VisaOfficialFeeKind Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Official fee kind is required.", nameof(value));
        }

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Official fee kind max length is {MaxLength}.", nameof(value));
        }

        return trimmed switch
        {
            ApplicationValue => Application,
            IssuanceValue => Issuance,
            EmbassyValue => Embassy,
            ServiceCenterValue => ServiceCenter,
            _ => throw new ArgumentException(
                $"Unknown Visa official fee kind '{trimmed}'. Allowed: {string.Join(", ", AllowedValues)}.",
                nameof(value)),
        };
    }

    public bool Equals(VisaOfficialFeeKind? other) =>
        other is not null && Value.Equals(other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is VisaOfficialFeeKind other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value;

    public static bool operator ==(VisaOfficialFeeKind? left, VisaOfficialFeeKind? right) => Equals(left, right);

    public static bool operator !=(VisaOfficialFeeKind? left, VisaOfficialFeeKind? right) => !Equals(left, right);
}
