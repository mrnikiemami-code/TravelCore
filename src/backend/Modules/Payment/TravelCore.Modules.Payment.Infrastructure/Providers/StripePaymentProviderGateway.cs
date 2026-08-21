using Microsoft.Extensions.Options;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Infrastructure.Options;

namespace TravelCore.Modules.Payment.Infrastructure.Providers;

/// <summary>
/// Stripe UAE TEST-MODE Payment provider adapter (TC-P35-T008).
/// Browser return is never success. NamedProductionAdapterImplemented stays false.
/// </summary>
internal sealed class StripePaymentProviderGateway : IPaymentProviderGateway
{
    public static readonly ProviderKey FixedKey = new(PaymentStripeGate.ProviderKeyValue);

    private static readonly HashSet<string> SupportedCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "AED",
        "USD",
    };

    private readonly IOptions<PaymentStripeOptions> _options;
    private readonly IStripeCheckoutClient _client;

    public StripePaymentProviderGateway(
        IOptions<PaymentStripeOptions> options,
        IStripeCheckoutClient client)
    {
        _options = options;
        _client = client;
    }

    public ProviderKey Key => FixedKey;

    public PaymentProviderCapability Capabilities =>
        PaymentProviderCapability.RedirectInitiation
        | PaymentProviderCapability.CallbackVerification
        | PaymentProviderCapability.PaymentStatusQuery
        | PaymentProviderCapability.RefundInitiation
        | PaymentProviderCapability.RefundStatusQuery;

    public async Task<PaymentInitiationResult> InitiatePaymentAsync(
        PaymentInitiationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        if (!SupportedCurrencies.Contains(request.CurrencyCode))
        {
            return new PaymentInitiationResult
            {
                Outcome = PaymentInitiationOutcome.DefinitiveFailure,
                ProviderKey = Key,
            };
        }

        if (!TryToMinorUnits(request.Amount, request.CurrencyCode, out var minor))
        {
            return new PaymentInitiationResult
            {
                Outcome = PaymentInitiationOutcome.DefinitiveFailure,
                ProviderKey = Key,
            };
        }

        var successUrl = ResolveReturnUrl(_options.Value.SuccessUrl, "success");
        var cancelUrl = ResolveReturnUrl(_options.Value.CancelUrl, "cancel");
        if (successUrl is null || cancelUrl is null)
        {
            return new PaymentInitiationResult
            {
                Outcome = PaymentInitiationOutcome.DefinitiveFailure,
                ProviderKey = Key,
            };
        }

        var clientReference = request.PaymentAttemptId.ToString("N");
        var session = await _client.CreateCheckoutSessionAsync(
            new StripeCheckoutSessionCreateRequest(
                minor,
                request.CurrencyCode.ToUpperInvariant(),
                clientReference,
                successUrl,
                cancelUrl,
                IdempotencyKey: $"pay-{request.PaymentAttemptId:N}",
                Metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["paymentAttemptId"] = request.PaymentAttemptId.ToString("D"),
                    ["bookingId"] = request.BookingId.ToString("D"),
                    ["paymentId"] = request.PaymentId.ToString("D"),
                }),
            cancellationToken);

        if (string.IsNullOrWhiteSpace(session.Url)
            || !Uri.TryCreate(session.Url, UriKind.Absolute, out var redirect))
        {
            return new PaymentInitiationResult
            {
                Outcome = PaymentInitiationOutcome.DefinitiveFailure,
                ProviderKey = Key,
                RequestReference = new ProviderRequestReference(session.SessionId),
            };
        }

        return new PaymentInitiationResult
        {
            Outcome = PaymentInitiationOutcome.Initiated,
            ProviderKey = Key,
            RequestReference = new ProviderRequestReference(session.SessionId),
            TransactionReference = string.IsNullOrWhiteSpace(session.PaymentIntentId)
                ? null
                : new ProviderTransactionReference(session.PaymentIntentId),
            RedirectUri = redirect,
        };
    }

    public Task<PaymentVerificationResult> VerifyPaymentAsync(
        PaymentVerificationRequest request,
        CancellationToken cancellationToken = default) =>
        QueryPaymentStatusAsync(request, cancellationToken);

    public async Task<PaymentVerificationResult> QueryPaymentStatusAsync(
        PaymentVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        if (request.RequestReference is null)
        {
            return Pending(request);
        }

        var session = await _client.GetCheckoutSessionAsync(request.RequestReference.Value.Value, cancellationToken);
        if (session is null)
        {
            return Pending(request);
        }

        return MapSessionToVerification(session, request);
    }

    public Task<PaymentCallbackVerification> VerifyCallbackAsync(
        PaymentCallbackEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(envelope);

        var webhookSecret = _options.Value.WebhookSecret;
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            return Task.FromResult(PaymentCallbackVerification.Unverified());
        }

        if (!envelope.Headers.TryGetValue(PaymentStripeGate.SignatureHeaderName, out var signature)
            || string.IsNullOrWhiteSpace(signature))
        {
            return Task.FromResult(PaymentCallbackVerification.Unverified());
        }

        var parsed = _client.ParseWebhookEvent(envelope.Body ?? string.Empty, signature, webhookSecret);
        if (!parsed.IsValid)
        {
            return Task.FromResult(PaymentCallbackVerification.Unverified());
        }

        var outcome = MapEventOutcome(parsed.EventType, parsed.PaymentStatus)
            ?? ProviderVerificationOutcome.PendingUnknown;

        ProviderRequestReference? requestReference = string.IsNullOrWhiteSpace(parsed.SessionId)
            ? null
            : new ProviderRequestReference(parsed.SessionId);
        ProviderTransactionReference? transactionReference = string.IsNullOrWhiteSpace(parsed.PaymentIntentId)
            ? null
            : new ProviderTransactionReference(parsed.PaymentIntentId);

        decimal? amount = null;
        if (parsed.AmountTotal is long minor && !string.IsNullOrWhiteSpace(parsed.CurrencyCode))
        {
            amount = FromMinorUnits(minor, parsed.CurrencyCode);
        }

        return Task.FromResult(PaymentCallbackVerification.Verified(new PaymentVerificationResult
        {
            Outcome = outcome,
            ProviderKey = Key,
            RequestReference = requestReference,
            TransactionReference = transactionReference,
            ReportedAmount = amount,
            ReportedCurrencyCode = string.IsNullOrWhiteSpace(parsed.CurrencyCode)
                ? null
                : parsed.CurrencyCode.ToUpperInvariant(),
        }));
    }

    public async Task<PaymentInitiationResult> InitiateRefundAsync(
        RefundInitiationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        if (request.OriginalPaymentTransactionReference is null
            || string.IsNullOrWhiteSpace(request.OriginalPaymentTransactionReference.Value.Value))
        {
            return new PaymentInitiationResult
            {
                Outcome = PaymentInitiationOutcome.DefinitiveFailure,
                ProviderKey = Key,
            };
        }

        if (!TryToMinorUnits(request.Amount, request.CurrencyCode, out var units))
        {
            return new PaymentInitiationResult
            {
                Outcome = PaymentInitiationOutcome.DefinitiveFailure,
                ProviderKey = Key,
            };
        }

        var refund = await _client.CreateRefundAsync(
            new StripeRefundCreateRequest(
                request.OriginalPaymentTransactionReference.Value.Value,
                units,
                IdempotencyKey: $"refund-{request.RefundAttemptId:N}"),
            cancellationToken);

        return new PaymentInitiationResult
        {
            Outcome = PaymentInitiationOutcome.Initiated,
            ProviderKey = Key,
            RequestReference = new ProviderRequestReference(refund.RefundId),
            TransactionReference = new ProviderTransactionReference(refund.RefundId),
        };
    }

    public Task<PaymentVerificationResult> VerifyRefundAsync(
        PaymentVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Status query against Stripe refund retrieve can be added later; v1 marks initiated as PendingUnknown until webhook.
        return Task.FromResult(new PaymentVerificationResult
        {
            Outcome = ProviderVerificationOutcome.PendingUnknown,
            ProviderKey = Key,
            RequestReference = request.RequestReference,
            TransactionReference = request.TransactionReference,
        });
    }

    public Task<PaymentVerificationResult> QueryRefundStatusAsync(
        PaymentVerificationRequest request,
        CancellationToken cancellationToken = default) =>
        VerifyRefundAsync(request, cancellationToken);

    internal static bool TryToMinorUnits(decimal amount, string currencyCode, out long minorUnits)
    {
        minorUnits = 0;
        if (amount < 0 || !SupportedCurrencies.Contains(currencyCode))
        {
            return false;
        }

        var scaled = amount * 100m;
        if (scaled != decimal.Truncate(scaled))
        {
            return false;
        }

        if (scaled > long.MaxValue)
        {
            return false;
        }

        minorUnits = (long)scaled;
        return true;
    }

    internal static decimal FromMinorUnits(long minorUnits, string currencyCode)
    {
        _ = currencyCode;
        return minorUnits / 100m;
    }

    private PaymentVerificationResult MapSessionToVerification(
        StripeCheckoutSessionResult session,
        PaymentVerificationRequest request)
    {
        var outcome = MapPaymentStatus(session.PaymentStatus, session.Status);
        decimal? amount = session.AmountTotal is long minor
            ? FromMinorUnits(minor, session.CurrencyCode ?? "USD")
            : null;

        return new PaymentVerificationResult
        {
            Outcome = outcome,
            ProviderKey = Key,
            RequestReference = new ProviderRequestReference(session.SessionId),
            TransactionReference = string.IsNullOrWhiteSpace(session.PaymentIntentId)
                ? request.TransactionReference
                : new ProviderTransactionReference(session.PaymentIntentId),
            ReportedAmount = amount,
            ReportedCurrencyCode = string.IsNullOrWhiteSpace(session.CurrencyCode)
                ? null
                : session.CurrencyCode.ToUpperInvariant(),
        };
    }

    private static ProviderVerificationOutcome MapPaymentStatus(string? paymentStatus, string? sessionStatus)
    {
        if (string.Equals(paymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
        {
            return ProviderVerificationOutcome.Succeeded;
        }

        if (string.Equals(sessionStatus, "expired", StringComparison.OrdinalIgnoreCase)
            || string.Equals(paymentStatus, "unpaid", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(sessionStatus, "expired", StringComparison.OrdinalIgnoreCase)
                ? ProviderVerificationOutcome.Failed
                : ProviderVerificationOutcome.PendingUnknown;
        }

        return ProviderVerificationOutcome.PendingUnknown;
    }

    private static ProviderVerificationOutcome? MapEventOutcome(string? eventType, string? paymentStatus)
    {
        if (string.Equals(eventType, "checkout.session.completed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventType, "payment_intent.succeeded", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(paymentStatus, "paid", StringComparison.OrdinalIgnoreCase)
                || string.Equals(paymentStatus, "succeeded", StringComparison.OrdinalIgnoreCase)
                || paymentStatus is null)
            {
                return ProviderVerificationOutcome.Succeeded;
            }
        }

        if (string.Equals(eventType, "checkout.session.expired", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventType, "payment_intent.payment_failed", StringComparison.OrdinalIgnoreCase))
        {
            return ProviderVerificationOutcome.Failed;
        }

        return null;
    }

    private PaymentVerificationResult Pending(PaymentVerificationRequest request) =>
        new()
        {
            Outcome = ProviderVerificationOutcome.PendingUnknown,
            ProviderKey = Key,
            RequestReference = request.RequestReference,
            TransactionReference = request.TransactionReference,
        };

    private string? ResolveReturnUrl(string? configured, string kind)
    {
        if (!string.IsNullOrWhiteSpace(configured)
            && Uri.TryCreate(configured.Trim(), UriKind.Absolute, out _))
        {
            return configured.Trim();
        }

        var baseUrl = _options.Value.PublicBaseUrl?.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return null;
        }

        // Browser return only — Payment success is webhook-owned.
        return $"{baseUrl}/api/payment/providers/{PaymentStripeGate.ProviderKeyValue}/browser-return?kind={Uri.EscapeDataString(kind)}";
    }
}
