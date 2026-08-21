namespace TravelCore.Modules.Payment.Infrastructure.Providers;

/// <summary>
/// Stripe Checkout / refund / retrieve ports. Keeps Stripe.net types out of Contracts (TC-P35-T008).
/// </summary>
internal interface IStripeCheckoutClient
{
    Task<StripeCheckoutSessionResult> CreateCheckoutSessionAsync(
        StripeCheckoutSessionCreateRequest request,
        CancellationToken cancellationToken = default);

    Task<StripeCheckoutSessionResult?> GetCheckoutSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    Task<StripeRefundResult> CreateRefundAsync(
        StripeRefundCreateRequest request,
        CancellationToken cancellationToken = default);

    StripeWebhookParseResult ParseWebhookEvent(string payload, string stripeSignatureHeader, string webhookSecret);
}

internal sealed record StripeCheckoutSessionCreateRequest(
    long AmountMinorUnits,
    string CurrencyCode,
    string ClientReferenceId,
    string SuccessUrl,
    string CancelUrl,
    string IdempotencyKey,
    IReadOnlyDictionary<string, string> Metadata);

internal sealed record StripeCheckoutSessionResult(
    string SessionId,
    string? PaymentIntentId,
    string? Url,
    string Status,
    string PaymentStatus,
    long? AmountTotal,
    string? CurrencyCode);

internal sealed record StripeRefundCreateRequest(
    string PaymentIntentId,
    long? AmountMinorUnits,
    string IdempotencyKey);

internal sealed record StripeRefundResult(
    string RefundId,
    string Status,
    long? Amount,
    string? CurrencyCode);

internal sealed record StripeWebhookParseResult(
    bool IsValid,
    string? EventId,
    string? EventType,
    string? SessionId,
    string? PaymentIntentId,
    string? PaymentStatus,
    long? AmountTotal,
    string? CurrencyCode,
    string? ClientReferenceId);
