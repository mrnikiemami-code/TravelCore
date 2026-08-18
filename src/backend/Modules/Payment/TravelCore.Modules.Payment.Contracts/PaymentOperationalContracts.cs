namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Internal read-only operational Payment/Refund facts (P20-R8).
/// Not a customer API. Not financial-truth authority. No passenger/contact/token.
/// </summary>
public static class PaymentOperationalBoundary
{
    public const bool PublicOperationalEndpointImplemented = false;
    public const bool ManualPaymentMutationImplemented = false;
    public const bool ManualRefundMutationImplemented = false;
    public const bool ManualBookingConfirmImplemented = false;
    public const string OperationalReadsAreNotTruthAuthority = "OperationalRead != FinancialTruthAuthority";
    public const string RecheckOutcomeSource = "AuthoritativeProviderQuery";
}

public sealed record PaymentAttemptOperationalRead(
    Guid AttemptId,
    string Status,
    string? ProviderKey,
    string? ProviderRequestReference,
    string? ProviderTransactionReference,
    DateTimeOffset CreatedAt,
    DateTimeOffset? InitiatedAt);

public sealed record RefundAttemptOperationalRead(
    Guid AttemptId,
    string Status,
    string? ProviderKey,
    string? ProviderRequestReference,
    string? ProviderTransactionReference,
    DateTimeOffset CreatedAt,
    DateTimeOffset? InitiatedAt);

public sealed record RefundOperationalRead(
    Guid RefundId,
    string Status,
    decimal Amount,
    string CurrencyCode,
    IReadOnlyList<RefundAttemptOperationalRead> Attempts,
    IReadOnlyList<string> ReconciliationKinds);

public sealed record PaymentOperationalRead(
    Guid PaymentId,
    Guid BookingId,
    string PaymentStatus,
    decimal? Amount,
    string? CurrencyCode,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SucceededAt,
    IReadOnlyList<PaymentAttemptOperationalRead> Attempts,
    RefundOperationalRead? Refund,
    IReadOnlyList<string> ReconciliationKinds,
    PaymentProviderDescriptor? Provider,
    string? CompensationState);

public interface IPaymentOperationalQuery
{
    Task<PaymentOperationalRead?> GetByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);

    Task<ProviderCapabilityStatus> RecheckPaymentAttemptAsync(
        Guid attemptId,
        CancellationToken cancellationToken = default);

    Task<ProviderCapabilityStatus> RecheckRefundAttemptAsync(
        Guid attemptId,
        CancellationToken cancellationToken = default);
}
