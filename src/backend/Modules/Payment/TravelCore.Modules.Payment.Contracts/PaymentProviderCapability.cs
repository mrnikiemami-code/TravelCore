namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Provider-declared capabilities. Payment core does not infer these from ProviderKey (P20-R8).
/// </summary>
[Flags]
public enum PaymentProviderCapability
{
    None = 0,
    RedirectInitiation = 1,
    CallbackVerification = 2,
    PaymentStatusQuery = 4,
    RefundInitiation = 8,
    RefundVerification = 16,
    RefundStatusQuery = 32,
}

/// <summary>
/// Safe provider descriptor. Secrets and merchant account details are excluded (P20-R8).
/// </summary>
public sealed record PaymentProviderDescriptor(
    string ProviderKey,
    string DisplayName,
    PaymentProviderCapability Capabilities,
    bool Enabled,
    bool AvailableForPublicInitiation);

public static class PaymentProviderCapabilitySet
{
    public const PaymentProviderCapability All =
        PaymentProviderCapability.RedirectInitiation
        | PaymentProviderCapability.CallbackVerification
        | PaymentProviderCapability.PaymentStatusQuery
        | PaymentProviderCapability.RefundInitiation
        | PaymentProviderCapability.RefundVerification
        | PaymentProviderCapability.RefundStatusQuery;

    public static readonly string[] ExactValues =
    [
        nameof(PaymentProviderCapability.RedirectInitiation),
        nameof(PaymentProviderCapability.CallbackVerification),
        nameof(PaymentProviderCapability.PaymentStatusQuery),
        nameof(PaymentProviderCapability.RefundInitiation),
        nameof(PaymentProviderCapability.RefundVerification),
        nameof(PaymentProviderCapability.RefundStatusQuery),
    ];
}

public enum ProviderCapabilityStatus
{
    Available = 0,
    UnknownProvider = 1,
    DisabledProvider = 2,
    UnsupportedCapability = 3,
}
