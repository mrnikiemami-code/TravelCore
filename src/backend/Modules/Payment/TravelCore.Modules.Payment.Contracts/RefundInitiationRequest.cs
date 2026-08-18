namespace TravelCore.Modules.Payment.Contracts;

/// <summary>
/// Trusted server-side refund initiation. Amount/currency are snapshot-owned, not event/client authored (P20-R6).
/// </summary>
public sealed record RefundInitiationRequest(
    Guid RefundId,
    Guid RefundAttemptId,
    Guid PaymentId,
    Guid BookingId,
    ProviderKey ProviderKey,
    ProviderTransactionReference? OriginalPaymentTransactionReference,
    decimal Amount,
    string CurrencyCode);
